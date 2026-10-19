using System.Globalization;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Data;
using ANDtr.Web.Models;

namespace ANDtr.Web.Services;

public class ImportService : IImportService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ImportService> _logger;

    public ImportService(AppDbContext db, IWebHostEnvironment env, ILogger<ImportService> logger)
    {
        _db = db;
        _env = env;
        _logger = logger;
    }

    public async Task<ImportResult> ImportFromDefaultFileAsync(string? userId, bool skipExisting = true)
    {
        var possiblePaths = new[]
        {
            Path.Combine(_env.ContentRootPath, "..", "..", "data", "FDAdataset.xlsx"),
            Path.Combine(_env.ContentRootPath, "data", "FDAdataset.xlsx"),
            Path.Combine(AppContext.BaseDirectory, "data", "FDAdataset.xlsx"),
            Path.Combine(_env.ContentRootPath, "FDAdataset.xlsx")
        };

        string? foundPath = possiblePaths.FirstOrDefault(File.Exists);
        if (foundPath == null)
        {
            return new ImportResult
            {
                Success = false,
                Message = "Default dataset file 'FDAdataset.xlsx' was not found in data directory."
            };
        }

        await using var stream = File.OpenRead(foundPath);
        return await ImportFromStreamAsync(stream, Path.GetFileName(foundPath), userId, skipExisting);
    }

    public async Task<ImportResult> ImportFromStreamAsync(Stream stream, string fileName, string? userId, bool skipExisting = true)
    {
        var result = new ImportResult();
        try
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                result.Success = false;
                result.Message = "The uploaded Excel file contains no worksheets.";
                return result;
            }

            // Find header row containing "RECALL_NUMBER"
            IXLRow? headerRow = null;
            var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in worksheet.RowsUsed())
            {
                var rowCells = row.CellsUsed().ToList();
                var hasRecallHeader = rowCells.Any(c => c.GetString().Trim().Equals("RECALL_NUMBER", StringComparison.OrdinalIgnoreCase));
                if (hasRecallHeader)
                {
                    headerRow = row;
                    foreach (var cell in rowCells)
                    {
                        var name = cell.GetString().Trim();
                        if (!string.IsNullOrEmpty(name) && !colMap.ContainsKey(name))
                        {
                            colMap[name] = cell.Address.ColumnNumber;
                        }
                    }
                    break;
                }
            }

            if (headerRow == null || !colMap.ContainsKey("RECALL_NUMBER"))
            {
                result.Success = false;
                result.Message = "Could not locate header row with 'RECALL_NUMBER' column in Sheet1.";
                return result;
            }

            int headerRowNum = headerRow.RowNumber();
            var parsedNotices = new List<RecallNotice>();
            var rawRecallNumbers = new List<string>();

            // First pass: extract all valid rows
            foreach (var row in worksheet.Rows(headerRowNum + 1, worksheet.LastRowUsed()?.RowNumber() ?? (headerRowNum + 1)))
            {
                if (row.IsEmpty()) continue;

                string GetVal(string colName)
                {
                    if (colMap.TryGetValue(colName, out var colIdx))
                    {
                        var cell = row.Cell(colIdx);
                        return cell.GetString().Trim();
                    }
                    return string.Empty;
                }

                var recallNumber = CleanText(GetVal("RECALL_NUMBER"));
                if (string.IsNullOrWhiteSpace(recallNumber)) continue;

                result.RowsRead++;
                rawRecallNumbers.Add(recallNumber);

                var productCode = CleanText(GetVal("PRODUCT CODE"));
                var productCodeName = CleanText(GetVal("Product Code Name"));
                var regNum = CleanText(GetVal("Regulation Number"));
                var webAddr = CleanText(GetVal("WEB_ADDRESS"));
                var description = CleanText(GetVal("PRODUCT_DESCRIPTION"), countCleaning: true, result: result);
                var tradeName = CleanText(GetVal("TRADE_NAME"), countCleaning: true, result: result);
                var recallClass = CleanText(GetVal("RECALL_CLASS"));
                var classificationDtStr = GetVal("CENTER_CLASSIFICATION_DT");
                var postedDtStr = GetVal("POSTED_INTERNET_DT");
                var termDtStr = GetVal("TERMINATION_DT");
                var fei = CleanText(GetVal("FEI_NUMBER"));
                var firm = CleanText(GetVal("FIRM_NAME"), countCleaning: true, result: result);
                var reason = CleanText(GetVal("MANUFACTURER_RECALL_REASON"), countCleaning: true, result: result);

                if (string.IsNullOrEmpty(tradeName)) result.BlankTradeName++;
                if (string.IsNullOrEmpty(fei)) result.BlankFeiNumber++;
                if (string.IsNullOrEmpty(postedDtStr)) result.BlankPostedDate++;
                if (string.IsNullOrEmpty(termDtStr)) result.BlankTerminationDate++;

                var classDate = ParseDate(classificationDtStr) ?? DateTime.UtcNow;
                var postedDate = ParseDate(postedDtStr);
                var termDate = ParseDate(termDtStr);

                // Standardize class value to "1", "2", or "3"
                var normClass = NormalizeClass(recallClass);

                var notice = new RecallNotice
                {
                    RecallNumber = recallNumber,
                    ProductCode = productCode,
                    ProductCodeName = productCodeName,
                    RegulationNumber = regNum,
                    WebAddress = webAddr,
                    ProductDescription = description,
                    TradeName = string.IsNullOrEmpty(tradeName) ? null : tradeName,
                    RecallClass = normClass,
                    ClassificationDate = classDate,
                    PostedDate = postedDate,
                    TerminationDate = termDate,
                    FeiNumber = string.IsNullOrEmpty(fei) ? null : fei,
                    FirmName = firm,
                    RecallReason = reason
                };

                parsedNotices.Add(notice);
            }

            // Detect duplicate recall numbers across the dataset
            var recallCounts = rawRecallNumbers.GroupBy(n => n).ToDictionary(g => g.Key, g => g.Count());
            result.DuplicatesFlagged = recallCounts.Count(kv => kv.Value > 1);

            foreach (var n in parsedNotices)
            {
                if (recallCounts.TryGetValue(n.RecallNumber, out var c) && c > 1)
                {
                    n.IsDuplicateRecallNumber = true;
                }
            }

            // Load existing notices from DB for idempotency
            var existingKeys = await _db.RecallNotices
                .Select(n => new { n.RecallNumber, n.ProductCode, n.ClassificationDate })
                .ToListAsync();

            var existingKeySet = new HashSet<string>(
                existingKeys.Select(k => $"{k.RecallNumber}|{k.ProductCode}|{k.ClassificationDate:yyyy-MM-dd}"),
                StringComparer.OrdinalIgnoreCase
            );

            var toInsert = new List<RecallNotice>();
            foreach (var notice in parsedNotices)
            {
                var key = $"{notice.RecallNumber}|{notice.ProductCode}|{notice.ClassificationDate:yyyy-MM-dd}";
                if (existingKeySet.Contains(key))
                {
                    if (skipExisting)
                    {
                        result.RowsSkipped++;
                        continue;
                    }
                }
                else
                {
                    existingKeySet.Add(key);
                }

                toInsert.Add(notice);
            }

            if (toInsert.Count > 0)
            {
                await _db.RecallNotices.AddRangeAsync(toInsert);
                result.RowsImported = toInsert.Count;
            }

            var record = new ImportRecord
            {
                FileName = fileName,
                ImportedAt = DateTime.UtcNow,
                ImportedById = userId,
                RowsRead = result.RowsRead,
                RowsImported = result.RowsImported,
                RowsSkipped = result.RowsSkipped,
                DuplicatesFlagged = result.DuplicatesFlagged,
                Status = "Complete",
                Details = $"{result.RowsImported} notices imported, {result.RowsSkipped} skipped, {result.DuplicatesFlagged} duplicate recall numbers flagged."
            };
            _db.ImportRecords.Add(record);

            await _db.SaveChangesAsync();

            result.Success = true;
            result.Message = $"Successfully imported {result.RowsImported} recall notices ({result.RowsSkipped} skipped).";
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing dataset from {FileName}", fileName);
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
            return result;
        }
    }

    public async Task<ImportStats> GetImportStatsAsync()
    {
        var notices = await _db.RecallNotices.ToListAsync();
        return new ImportStats
        {
            TotalNotices = notices.Count,
            Class1Count = notices.Count(n => n.RecallClass == "1"),
            Class2Count = notices.Count(n => n.RecallClass == "2"),
            Class3Count = notices.Count(n => n.RecallClass == "3"),
            TotalFirms = notices.Select(n => n.FirmName).Where(f => !string.IsNullOrEmpty(f)).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            TotalProductCodes = notices.Select(n => n.ProductCode).Where(p => !string.IsNullOrEmpty(p)).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            DuplicateRecallNumbersCount = notices.Where(n => n.IsDuplicateRecallNumber).Select(n => n.RecallNumber).Distinct().Count(),
            BlankPostedCount = notices.Count(n => n.PostedDate == null),
            BlankTerminationCount = notices.Count(n => n.TerminationDate == null),
            BlankTradeNameCount = notices.Count(n => string.IsNullOrEmpty(n.TradeName)),
            BlankFeiCount = notices.Count(n => string.IsNullOrEmpty(n.FeiNumber))
        };
    }

    public async Task<List<ImportRecord>> GetRecentImportsAsync(int count = 10)
    {
        return await _db.ImportRecords
            .Include(ir => ir.ImportedBy)
            .OrderByDescending(ir => ir.ImportedAt)
            .Take(count)
            .ToListAsync();
    }

    private static string CleanText(string input, bool countCleaning = false, ImportResult? result = null)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        var cleaned = input;
        bool hadArtifacts = cleaned.Contains("_x000D_") || cleaned.Contains("\r");

        cleaned = cleaned.Replace("_x000D_\n", "\n")
                         .Replace("_x000D_", "\n")
                         .Replace("\r\n", "\n")
                         .Replace("\r", "\n");

        cleaned = Regex.Replace(cleaned, @"\n{3,}", "\n\n").Trim();

        if (hadArtifacts && countCleaning && result != null)
        {
            result.TextFieldsCleaned++;
        }

        return cleaned;
    }

    private static DateTime? ParseDate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var cleaned = input.Trim();
        string[] formats = { "MM/dd/yyyy", "yyyy-MM-dd", "M/d/yyyy", "d/M/yyyy", "MM/dd/yy", "yyyy/MM/dd", "dd/MM/yyyy" };

        if (DateTime.TryParseExact(cleaned, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            return dt;
        }

        if (DateTime.TryParse(cleaned, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
        {
            return dt;
        }

        return null;
    }

    private static string NormalizeClass(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "2";
        raw = raw.Trim().ToUpperInvariant();
        if (raw == "1" || raw == "I" || raw.Contains("CLASS I") && !raw.Contains("II") && !raw.Contains("III")) return "1";
        if (raw == "2" || raw == "II" || raw.Contains("CLASS II")) return "2";
        if (raw == "3" || raw == "III" || raw.Contains("CLASS III")) return "3";
        return raw;
    }
}
