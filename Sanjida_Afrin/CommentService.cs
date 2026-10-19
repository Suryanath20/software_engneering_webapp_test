using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Data;
using ANDtr.Web.Models;

namespace ANDtr.Web.Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _db;

    public CommentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Comment> AddCommentAsync(int reviewId, string authorId, string body, string authorRole)
    {
        var comment = new Comment
        {
            ReviewId = reviewId,
            AuthorId = authorId,
            Body = body.Trim(),
            AuthorRole = authorRole,
            CreatedAt = DateTime.UtcNow
        };

        _db.Comments.Add(comment);

        var review = await _db.RiskReviews.FindAsync(reviewId);
        if (review != null)
        {
            review.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return comment;
    }

    public async Task<List<CommentDto>> GetCommentsForReviewAsync(int reviewId)
    {
        return await _db.Comments
            .Include(c => c.Author)
            .Include(c => c.Review)
            .Where(c => c.ReviewId == reviewId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                ReviewId = c.ReviewId,
                ReviewTitle = c.Review.Title,
                AuthorId = c.AuthorId,
                AuthorName = c.Author.FullName,
                AuthorCompany = c.Author.CompanyName,
                AuthorRole = c.AuthorRole,
                Body = c.Body,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<ReviewCommentGroupDto>> GetGroupedCustomerCommentsAsync(bool awaitingReplyOnly = false)
    {
        var reviewsWithComments = await _db.RiskReviews
            .Include(r => r.Comments)
                .ThenInclude(c => c.Author)
            .Where(r => r.Comments.Any())
            .ToListAsync();

        var groups = new List<ReviewCommentGroupDto>();

        foreach (var review in reviewsWithComments)
        {
            var orderedComments = review.Comments.OrderBy(c => c.CreatedAt).ToList();
            if (orderedComments.Count == 0) continue;

            // Awaiting reply condition: last comment in thread is from Customer
            bool isAwaiting = orderedComments.Last().AuthorRole == "Customer";
            if (awaitingReplyOnly && !isAwaiting) continue;

            groups.Add(new ReviewCommentGroupDto
            {
                ReviewId = review.Id,
                ReviewTitle = review.Title,
                Comments = orderedComments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    ReviewId = c.ReviewId,
                    ReviewTitle = review.Title,
                    AuthorId = c.AuthorId,
                    AuthorName = c.Author != null ? c.Author.FullName : "User",
                    AuthorCompany = c.Author?.CompanyName,
                    AuthorRole = c.AuthorRole,
                    Body = c.Body,
                    CreatedAt = c.CreatedAt
                }).ToList()
            });
        }

        return groups;
    }

    public async Task<int> GetTotalCustomerCommentsCountAsync()
    {
        return await _db.Comments.CountAsync(c => c.AuthorRole == "Customer");
    }

    public async Task<int> GetAwaitingReplyCountAsync()
    {
        var groups = await GetGroupedCustomerCommentsAsync(awaitingReplyOnly: true);
        return groups.Count;
    }
}
