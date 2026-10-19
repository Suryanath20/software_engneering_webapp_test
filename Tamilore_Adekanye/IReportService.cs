using ANDtr.Web.Models;

namespace ANDtr.Web.Services;

public class ReportNoticeItemDto
{
    public string RecallNumber { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string FirmName { get; set; } = string.Empty;
    public string RecallClass { get; set; } = string.Empty;
    public string RecallClassDisplay { get; set; } = string.Empty;
    public DateTime ClassificationDate { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string RegulationNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? EmployeeNote { get; set; }
}

public class RiskReviewReportDto
{
    public int ReviewId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? FindingsHtml { get; set; }
    public string Status { get; set; } = "Draft";
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public List<ReportNoticeItemDto> Notices { get; set; } = new();
    public int Class1Count => Notices.Count(n => n.RecallClass == "1");
    public int Class2Count => Notices.Count(n => n.RecallClass == "2");
    public int Class3Count => Notices.Count(n => n.RecallClass == "3");
}

public interface IReportService
{
    Task<RiskReviewReportDto?> GetReportDataAsync(int reviewId);
    Task<byte[]> GenerateCsvExportAsync(int reviewId);
}
