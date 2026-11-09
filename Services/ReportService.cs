using System.Text;
using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Data;
using ANDtr.Web.Models;

namespace ANDtr.Web.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<RiskReviewReportDto?> GetReportDataAsync(int reviewId)
    {
        var review = await _db.RiskReviews
            .Include(r => r.CreatedBy)
            .Include(r => r.ReviewNotices)
                .ThenInclude(rn => rn.Notice)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null) return null;

        return new RiskReviewReportDto
        {
            ReviewId = review.Id,
            Title = review.Title,
            Summary = review.Summary,
            FindingsHtml = review.FindingsHtml,
            Status = review.Status,
            AuthorName = review.CreatedBy != null ? review.CreatedBy.FullName : "ANDtr Risk Team",
            CreatedAt = review.CreatedAt,
            GeneratedAt = DateTime.UtcNow,
            Notices = review.ReviewNotices.Select(rn => new ReportNoticeItemDto
            {
                RecallNumber = rn.Notice.RecallNumber,
                TradeName = rn.Notice.TradeName ?? rn.Notice.ProductCodeName,
                FirmName = rn.Notice.FirmName,
                RecallClass = rn.Notice.RecallClass,
                RecallClassDisplay = rn.Notice.RecallClassDisplay,
                ClassificationDate = rn.Notice.ClassificationDate,
                ProductCode = rn.Notice.ProductCode,
                RegulationNumber = rn.Notice.RegulationNumber,
                Description = rn.Notice.ProductDescription,
                Reason = rn.Notice.RecallReason,
                EmployeeNote = rn.EmployeeNote
            }).ToList()
        };
    }

    public async Task<byte[]> GenerateCsvExportAsync(int reviewId)
    {
        var report = await GetReportDataAsync(reviewId);
        if (report == null) return Array.Empty<byte>();

        var sb = new StringBuilder();
        sb.AppendLine("Recall Number,Trade Name,Firm Name,Recall Class,Classification Date,Product Code,Regulation Number,Employee Note,Reason");

        foreach (var n in report.Notices)
        {
            sb.AppendLine(string.Join(",",
                EscapeCsv(n.RecallNumber),
                EscapeCsv(n.TradeName),
                EscapeCsv(n.FirmName),
                EscapeCsv(n.RecallClassDisplay),
                n.ClassificationDate.ToString("yyyy-MM-dd"),
                EscapeCsv(n.ProductCode),
                EscapeCsv(n.RegulationNumber),
                EscapeCsv(n.EmployeeNote ?? string.Empty),
                EscapeCsv(n.Reason)
            ));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string? field)
    {
        if (string.IsNullOrEmpty(field)) return "\"\"";
        var escaped = field.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}
