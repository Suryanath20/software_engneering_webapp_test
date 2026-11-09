using ANDtr.Web.Models;

namespace ANDtr.Web.Services;

public class ImportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RowsRead { get; set; }
    public int RowsImported { get; set; }
    public int RowsSkipped { get; set; }
    public int DuplicatesFlagged { get; set; }
    public int TextFieldsCleaned { get; set; }
    public int BlankPostedDate { get; set; }
    public int BlankTerminationDate { get; set; }
    public int BlankTradeName { get; set; }
    public int BlankFeiNumber { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public class ImportStats
{
    public int TotalNotices { get; set; }
    public int Class1Count { get; set; }
    public int Class2Count { get; set; }
    public int Class3Count { get; set; }
    public int TotalFirms { get; set; }
    public int TotalProductCodes { get; set; }
    public int DuplicateRecallNumbersCount { get; set; }
    public int BlankPostedCount { get; set; }
    public int BlankTerminationCount { get; set; }
    public int BlankTradeNameCount { get; set; }
    public int BlankFeiCount { get; set; }
}

public interface IImportService
{
    Task<ImportResult> ImportFromStreamAsync(Stream stream, string fileName, string? userId, bool skipExisting = true);
    Task<ImportResult> ImportFromDefaultFileAsync(string? userId, bool skipExisting = true);
    Task<ImportStats> GetImportStatsAsync();
    Task<List<ImportRecord>> GetRecentImportsAsync(int count = 10);
}
