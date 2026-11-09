using ANDtr.Web.Models;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Reports;

public class ReportViewModel
{
    public int? SelectedReviewId { get; set; }
    public List<RiskReview> AvailableReviews { get; set; } = new();
    public RiskReviewReportDto? ReportData { get; set; }
}
