using ANDtr.Web.Models;

namespace ANDtr.Web.Features.Notices;

public class NoticeDetailsViewModel
{
    public RecallNotice Notice { get; set; } = null!;
    public List<RiskReview> AssociatedReviews { get; set; } = new();
}
