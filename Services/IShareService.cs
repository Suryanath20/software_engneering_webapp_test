using ANDtr.Web.Models;

namespace ANDtr.Web.Services;

public class SharedReviewItemDto
{
    public int ShareId { get; set; }
    public int ReviewId { get; set; }
    public string ReviewTitle { get; set; } = string.Empty;
    public string SharedByName { get; set; } = string.Empty;
    public DateTime SharedAt { get; set; }
    public int NoticeCount { get; set; }
    public int CustomerCommentCount { get; set; }
    public string Status { get; set; } = "New";
}

public class ActiveShareItemDto
{
    public int ShareId { get; set; }
    public int ReviewId { get; set; }
    public string ReviewTitle { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCompany { get; set; } = string.Empty;
    public DateTime SharedAt { get; set; }
    public bool IsActive { get; set; }
}

public interface IShareService
{
    Task<(bool Success, string Message)> ShareReviewAsync(int reviewId, string customerId, string sharedById, string? message = null);
    Task<bool> StopSharingAsync(int shareId, string employeeId);
    Task<List<ActiveShareItemDto>> GetActiveSharesAsync(int? reviewId = null);
    Task<List<SharedReviewItemDto>> GetSharedReviewsForCustomerAsync(string customerId, string? search = null);
    Task<bool> CanCustomerAccessReviewAsync(string customerId, int reviewId);
    Task<int> GetCustomerSharedCountAsync(string customerId);
}
