using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Reviews;

[Authorize(Roles = "Employee")]
public class ReviewsController : Controller
{
    private readonly IReviewService _reviewService;
    private readonly INoticeSearchService _searchService;

    public ReviewsController(IReviewService reviewService, INoticeSearchService searchService)
    {
        _reviewService = reviewService;
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search = null, string? sharingFilter = null)
    {
        var reviews = await _reviewService.GetReviewsAsync(null, search, sharingFilter);

        var model = new ReviewsListViewModel
        {
            Search = search,
            SharingFilter = sharingFilter ?? "All reviews",
            Reviews = reviews
        };

        return View("Views/Index", model);
    }

    [HttpGet]
    public async Task<IActionResult> Create([FromQuery] List<int>? noticeIds)
    {
        var model = new CreateReviewViewModel();
        if (noticeIds != null && noticeIds.Count > 0)
        {
            model.NoticeIds = noticeIds;
            model.SelectedNotices = await _searchService.GetNoticesByIdsAsync(noticeIds);
        }

        return View("Views/Create", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReviewViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Title))
        {
            ModelState.AddModelError("Title", "A title is required before you can save.");
        }

        if (!ModelState.IsValid)
        {
            if (model.NoticeIds != null && model.NoticeIds.Count > 0)
            {
                model.SelectedNotices = await _searchService.GetNoticesByIdsAsync(model.NoticeIds);
            }
            return View("Views/Create", model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var review = await _reviewService.CreateReviewAsync(
            model.Title,
            model.Description,
            model.NoticeIds ?? new List<int>(),
            userId
        );

        TempData["Success"] = "Risk review created successfully. You can now add findings or share with customers.";
        return RedirectToAction("Index", "Findings", new { reviewId = review.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var review = await _reviewService.GetReviewByIdAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        var model = new EditReviewViewModel
        {
            Id = review.Id,
            Title = review.Title,
            Description = review.Summary,
            Status = review.Status,
            Notices = review.ReviewNotices.ToList()
        };

        return View("Views/Edit", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditReviewViewModel model, Dictionary<int, string>? notes)
    {
        if (string.IsNullOrWhiteSpace(model.Title))
        {
            ModelState.AddModelError("Title", "A title is required.");
        }

        if (!ModelState.IsValid)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review != null)
            {
                model.Notices = review.ReviewNotices.ToList();
            }
            return View("Views/Edit", model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        await _reviewService.UpdateReviewAsync(id, model.Title, model.Description, userId);

        // Update per-notice notes if provided
        if (notes != null)
        {
            foreach (var kv in notes)
            {
                await _reviewService.UpdateNoticeNoteAsync(id, kv.Key, kv.Value);
            }
        }

        TempData["Success"] = "Risk review updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveNotice(int reviewId, int noticeId)
    {
        await _reviewService.RemoveNoticeFromReviewAsync(reviewId, noticeId);
        TempData["Success"] = "Notice removed from review.";
        return RedirectToAction(nameof(Edit), new { id = reviewId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var deleted = await _reviewService.DeleteReviewAsync(id, userId);

        if (deleted)
        {
            TempData["Success"] = "Risk review deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Review could not be deleted.";
        }

        return RedirectToAction(nameof(Index));
    }
}
