using ANDtr.Web.Models;

namespace ANDtr.Web.Services;

public class ReviewCommentGroupDto
{
    public int ReviewId { get; set; }
    public string ReviewTitle { get; set; } = string.Empty;
    public List<CommentDto> Comments { get; set; } = new();
}

public class CommentDto
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public string ReviewTitle { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorCompany { get; set; }
    public string AuthorRole { get; set; } = "Employee";
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public interface ICommentService
{
    Task<Comment> AddCommentAsync(int reviewId, string authorId, string body, string authorRole);
    Task<List<CommentDto>> GetCommentsForReviewAsync(int reviewId);
    Task<List<ReviewCommentGroupDto>> GetGroupedCustomerCommentsAsync(bool awaitingReplyOnly = false);
    Task<int> GetTotalCustomerCommentsCountAsync();
    Task<int> GetAwaitingReplyCountAsync();
}
