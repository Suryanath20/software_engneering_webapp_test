using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Data;
using ANDtr.Web.Models;

namespace ANDtr.Web.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _db;
    private readonly HtmlSanitizer _sanitizer;

    public ReviewService(AppDbContext db)
    {
        _db = db;
        _sanitizer = new HtmlSanitizer();
        // Allow formatting tags commonly used in rich text findings
        _sanitizer.AllowedTags.Add("b");
        _sanitizer.AllowedTags.Add("strong");
        _sanitizer.AllowedTags.Add("i");
        _sanitizer.AllowedTags.Add("em");
        _sanitizer.AllowedTags.Add("u");
        _sanitizer.AllowedTags.Add("p");
        _sanitizer.AllowedTags.Add("br");
        _sanitizer.AllowedTags.Add("ul");
        _sanitizer.AllowedTags.Add("ol");
        _sanitizer.AllowedTags.Add("li");
        _sanitizer.AllowedTags.Add("span");
    }

    public async Task<List<ReviewDto>> GetReviewsAsync(string? userId = null, string? search = null, string? sharingFilter = null)
    {
        var query = _db.RiskReviews
            .Include(r => r.CreatedBy)
            .Include(r => r.ReviewNotices)
            .Include(r => r.Shares)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(r => r.CreatedById == userId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(r => r.Title.Contains(s) || (r.Summary != null && r.Summary.Contains(s)));
        }

        var list = await query
            .OrderByDescending(r => r.UpdatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                Title = r.Title,
                Summary = r.Summary,
                FindingsHtml = r.FindingsHtml,
                Status = r.Status,
                CreatedById = r.CreatedById,
                CreatedByName = r.CreatedBy != null ? r.CreatedBy.FullName : "Unknown",
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                NoticeCount = r.ReviewNotices.Count,
                ShareCount = r.Shares.Count(s => s.RevokedAt == null)
            })
            .ToListAsync();

        if (sharingFilter?.Equals("Shared", StringComparison.OrdinalIgnoreCase) == true)
        {
            list = list.Where(r => r.IsShared).ToList();
        }
        else if (sharingFilter?.Equals("Not shared", StringComparison.OrdinalIgnoreCase) == true ||
                 sharingFilter?.Equals("NotShared", StringComparison.OrdinalIgnoreCase) == true)
        {
            list = list.Where(r => !r.IsShared).ToList();
        }

        return list;
    }

    public async Task<RiskReview?> GetReviewByIdAsync(int id)
    {
        return await _db.RiskReviews
            .Include(r => r.CreatedBy)
            .Include(r => r.ReviewNotices)
                .ThenInclude(rn => rn.Notice)
            .Include(r => r.Shares.Where(s => s.RevokedAt == null))
                .ThenInclude(s => s.Customer)
            .Include(r => r.Comments.OrderBy(c => c.CreatedAt))
                .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<RiskReview> CreateReviewAsync(string title, string? summary, IEnumerable<int> noticeIds, string userId)
    {
        var review = new RiskReview
        {
            Title = title.Trim(),
            Summary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim(),
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = "Draft"
        };

        var distinctNoticeIds = noticeIds.Distinct().ToList();
        foreach (var nid in distinctNoticeIds)
        {
            review.ReviewNotices.Add(new ReviewNotice
            {
                NoticeId = nid,
                AddedAt = DateTime.UtcNow
            });
        }

        _db.RiskReviews.Add(review);
        await _db.SaveChangesAsync();
        return review;
    }

    public async Task<bool> UpdateReviewAsync(int id, string title, string? summary, string userId)
    {
        var review = await _db.RiskReviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review == null) return false;

        review.Title = title.Trim();
        review.Summary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim();
        review.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateFindingsAsync(int id, string? findingsHtml, string? status, string userId)
    {
        var review = await _db.RiskReviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review == null) return false;

        if (findingsHtml != null)
        {
            review.FindingsHtml = _sanitizer.Sanitize(findingsHtml);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            review.Status = status;
        }

        review.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateNoticeNoteAsync(int reviewId, int noticeId, string? note)
    {
        var rn = await _db.ReviewNotices
            .FirstOrDefaultAsync(x => x.ReviewId == reviewId && x.NoticeId == noticeId);
        if (rn == null) return false;

        rn.EmployeeNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddNoticesToReviewAsync(int reviewId, IEnumerable<int> noticeIds)
    {
        var review = await _db.RiskReviews
            .Include(r => r.ReviewNotices)
            .FirstOrDefaultAsync(r => r.Id == reviewId);
        if (review == null) return false;

        var existing = review.ReviewNotices.Select(x => x.NoticeId).ToHashSet();
        foreach (var nid in noticeIds)
        {
            if (!existing.Contains(nid))
            {
                review.ReviewNotices.Add(new ReviewNotice
                {
                    ReviewId = reviewId,
                    NoticeId = nid,
                    AddedAt = DateTime.UtcNow
                });
            }
        }

        review.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveNoticeFromReviewAsync(int reviewId, int noticeId)
    {
        var rn = await _db.ReviewNotices
            .FirstOrDefaultAsync(x => x.ReviewId == reviewId && x.NoticeId == noticeId);
        if (rn == null) return false;

        _db.ReviewNotices.Remove(rn);
        var review = await _db.RiskReviews.FindAsync(reviewId);
        if (review != null) review.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteReviewAsync(int id, string userId)
    {
        var review = await _db.RiskReviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review == null) return false;

        _db.RiskReviews.Remove(review);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetEmployeeReviewCountAsync(string userId)
    {
        return await _db.RiskReviews.CountAsync(r => r.CreatedById == userId);
    }

    public async Task<int> GetSharedReviewCountAsync(string? userId = null)
    {
        var query = _db.ReviewShares.Where(s => s.RevokedAt == null);
        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(s => s.SharedById == userId);
        }
        return await query.Select(s => s.ReviewId).Distinct().CountAsync();
    }
}
