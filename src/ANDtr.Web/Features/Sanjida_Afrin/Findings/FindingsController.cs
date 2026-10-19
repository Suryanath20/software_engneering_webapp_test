using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Findings;

[Authorize(Roles = "Employee")]
public class FindingsController : Controller
{
    private readonly IReviewService _reviewService;
    private readonly ICommentService _commentService;
    private readonly INoticeSearchService _searchService;

    public FindingsController(
        IReviewService reviewService,
        ICommentService commentService,
        INoticeSearchService searchService)
    {
        _reviewService = reviewService;
        _commentService = commentService;
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int reviewId, int? noticeId = null)
    {
        var review = await _reviewService.GetReviewByIdAsync(reviewId);
        if (review == null)
        {
            return NotFound();
        }

        var activeNotice = noticeId.HasValue
            ? review.ReviewNotices.FirstOrDefault(rn => rn.NoticeId == noticeId.Value)?.Notice
            : review.ReviewNotices.FirstOrDefault()?.Notice;

        var comments = await _commentService.GetCommentsForReviewAsync(reviewId);
        var activeShares = review.Shares.Count(s => s.RevokedAt == null);

        var model = new FindingsViewModel
        {
            Review = review,
            ActiveNotice = activeNotice,
            Comments = comments,
            ActiveShareCount = activeShares
        };

        return View("Views/Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveComment(int reviewId, string body, int? noticeId = null)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            TempData["Error"] = "Enter a comment before saving.";
            return RedirectToAction(nameof(Index), new { reviewId, noticeId });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        await _commentService.AddCommentAsync(reviewId, userId, body, "Employee");

        TempData["Success"] = "Comment saved successfully.";
        return RedirectToAction(nameof(Index), new { reviewId, noticeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateFindings(int reviewId, string? findingsHtml, string? status, int? noticeId = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        await _reviewService.UpdateFindingsAsync(reviewId, findingsHtml, status, userId);

        TempData["Success"] = "Findings and review status updated.";
        return RedirectToAction(nameof(Index), new { reviewId, noticeId });
    }
}
