using ANDtr.Web.Models;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.SharedReviews;

public class SharedListViewModel
{
    public string? Search { get; set; }
    public List<SharedReviewItemDto> SharedReviews { get; set; } = new();
}

public class SharedReviewViewModel
{
    public RiskReview Review { get; set; } = null!;
    public List<CommentDto> Comments { get; set; } = new();
    public string? NewCommentBody { get; set; }
}

public class CustomerCommentsViewModel
{
    public int TotalCustomerComments { get; set; }
    public int AwaitingReplyCount { get; set; }
    public int ReviewsWithCommentsCount { get; set; }
    public bool AwaitingReplyOnly { get; set; }
    public List<ReviewCommentGroupDto> GroupedComments { get; set; } = new();
}
