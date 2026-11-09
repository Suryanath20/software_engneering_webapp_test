using ANDtr.Web.Models;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Sharing;

public class ShareViewModel
{
    public int? SelectedReviewId { get; set; }
    public string? SelectedCustomerId { get; set; }
    public string? Message { get; set; }

    public List<RiskReview> AvailableReviews { get; set; } = new();
    public List<ApplicationUser> AvailableCustomers { get; set; } = new();

    public RiskReview? SelectedReview { get; set; }
    public List<ActiveShareItemDto> CurrentShares { get; set; } = new();
}
