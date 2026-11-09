using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Data;
using ANDtr.Web.Models;

namespace ANDtr.Web.Services;

public class ShareService : IShareService
{
    private readonly AppDbContext _db;

    public ShareService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(bool Success, string Message)> ShareReviewAsync(int reviewId, string customerId, string sharedById, string? message = null)
    {
        var review = await _db.RiskReviews.FindAsync(reviewId);
        if (review == null)
        {
            return (false, "Review not found.");
        }

        var customer = await _db.Users.FindAsync(customerId);
        if (customer == null || !customer.IsActive)
        {
            return (false, "Customer not found or inactive.");
        }

        // Check if an active share already exists
        var existingShare = await _db.ReviewShares
            .FirstOrDefaultAsync(s => s.ReviewId == reviewId && s.CustomerId == customerId && s.RevokedAt == null);

        if (existingShare != null)
        {
            return (false, "This review is already actively shared with this customer.");
        }

        var share = new ReviewShare
        {
            ReviewId = reviewId,
            CustomerId = customerId,
            SharedById = sharedById,
            SharedAt = DateTime.UtcNow,
            Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim(),
            RevokedAt = null
        };

        _db.ReviewShares.Add(share);
        await _db.SaveChangesAsync();

        return (true, "The review has been shared. The customer can now open it from Shared with Me.");
    }

    public async Task<bool> StopSharingAsync(int shareId, string employeeId)
    {
        var share = await _db.ReviewShares.FindAsync(shareId);
        if (share == null || share.RevokedAt != null)
        {
            return false;
        }

        share.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<ActiveShareItemDto>> GetActiveSharesAsync(int? reviewId = null)
    {
        var query = _db.ReviewShares
            .Include(s => s.Review)
            .Include(s => s.Customer)
            .AsNoTracking()
            .AsQueryable();

        if (reviewId.HasValue)
        {
            query = query.Where(s => s.ReviewId == reviewId.Value);
        }

        return await query
            .OrderByDescending(s => s.SharedAt)
            .Select(s => new ActiveShareItemDto
            {
                ShareId = s.Id,
                ReviewId = s.ReviewId,
                ReviewTitle = s.Review.Title,
                CustomerId = s.CustomerId,
                CustomerName = s.Customer.FullName,
                CustomerCompany = s.Customer.CompanyName ?? string.Empty,
                SharedAt = s.SharedAt,
                IsActive = s.RevokedAt == null
            })
            .ToListAsync();
    }

    public async Task<List<SharedReviewItemDto>> GetSharedReviewsForCustomerAsync(string customerId, string? search = null)
    {
        var query = _db.ReviewShares
            .Include(s => s.Review)
                .ThenInclude(r => r.ReviewNotices)
            .Include(s => s.Review)
                .ThenInclude(r => r.Comments)
            .Include(s => s.SharedBy)
            .Where(s => s.CustomerId == customerId && s.RevokedAt == null)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(sh => sh.Review.Title.Contains(s) || (sh.Review.Summary != null && sh.Review.Summary.Contains(s)));
        }

        var shares = await query
            .OrderByDescending(s => s.SharedAt)
            .ToListAsync();

        return shares.Select(s => new SharedReviewItemDto
        {
            ShareId = s.Id,
            ReviewId = s.ReviewId,
            ReviewTitle = s.Review.Title,
            SharedByName = !string.IsNullOrWhiteSpace(s.SharedBy.FullName) ? s.SharedBy.FullName : "ANDtr Risk Team",
            SharedAt = s.SharedAt,
            NoticeCount = s.Review.ReviewNotices.Count,
            CustomerCommentCount = s.Review.Comments.Count(c => c.AuthorId == customerId),
            Status = (DateTime.UtcNow - s.SharedAt).TotalDays < 3 ? "New" : "Viewed"
        }).ToList();
    }

    public async Task<bool> CanCustomerAccessReviewAsync(string customerId, int reviewId)
    {
        return await _db.ReviewShares
            .AnyAsync(s => s.CustomerId == customerId && s.ReviewId == reviewId && s.RevokedAt == null);
    }

    public async Task<int> GetCustomerSharedCountAsync(string customerId)
    {
        return await _db.ReviewShares
            .CountAsync(s => s.CustomerId == customerId && s.RevokedAt == null);
    }
}
