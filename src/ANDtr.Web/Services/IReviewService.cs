using ANDtr.Web.Models;

namespace ANDtr.Web.Services;

public class ReviewDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? FindingsHtml { get; set; }
    public string Status { get; set; } = "Draft";
    public string CreatedById { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int NoticeCount { get; set; }
    public int ShareCount { get; set; }
    public bool IsShared => ShareCount > 0;
}

public interface IReviewService
{
    Task<List<ReviewDto>> GetReviewsAsync(string? userId = null, string? search = null, string? sharingFilter = null);
    Task<RiskReview?> GetReviewByIdAsync(int id);
    Task<RiskReview> CreateReviewAsync(string title, string? summary, IEnumerable<int> noticeIds, string userId);
    Task<bool> UpdateReviewAsync(int id, string title, string? summary, string userId);
    Task<bool> UpdateFindingsAsync(int id, string? findingsHtml, string? status, string userId);
    Task<bool> UpdateNoticeNoteAsync(int reviewId, int noticeId, string? note);
    Task<bool> AddNoticesToReviewAsync(int reviewId, IEnumerable<int> noticeIds);
    Task<bool> RemoveNoticeFromReviewAsync(int reviewId, int noticeId);
    Task<bool> DeleteReviewAsync(int id, string userId);
    Task<int> GetEmployeeReviewCountAsync(string userId);
    Task<int> GetSharedReviewCountAsync(string? userId = null);
}
