using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Dashboard;

public class CustomerDashboardViewModel
{
    public string CustomerName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public int SharedReviewsCount { get; set; }
    public int NewSinceLastVisitCount { get; set; }
    public int CustomerCommentsCount { get; set; }
    public List<SharedReviewItemDto> SharedReviews { get; set; } = new();
}
