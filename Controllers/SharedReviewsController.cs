using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.SharedReviews;

[Authorize]
public class SharedReviewsController : Controller
{
    private readonly IShareService _shareService;
    private readonly IReviewService _reviewService;
    private readonly ICommentService _commentService;

    public SharedReviewsController(
        IShareService shareService,
        IReviewService reviewService,
        ICommentService commentService)
    {
        _shareService = shareService;
        _reviewService = reviewService;
        _commentService = commentService;
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Index(string? search = null)
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var sharedReviews = await _shareService.GetSharedReviewsForCustomerAsync(customerId, search);

        var model = new SharedListViewModel
        {
            Search = search,
            SharedReviews = sharedReviews
        };

        return View("SharedList", model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var isCustomer = User.IsInRole("Customer");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        if (isCustomer)
        {
            var canAccess = await _shareService.CanCustomerAccessReviewAsync(userId, id);
            if (!canAccess)
            {
                return RedirectToAction("AccessDenied", "Accounts");
            }
        }

        var review = await _reviewService.GetReviewByIdAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        var comments = await _commentService.GetCommentsForReviewAsync(id);

        var model = new SharedReviewViewModel
        {
            Review = review,
            Comments = comments
        };

        return View("SharedReview", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int reviewId, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            TempData["Error"] = "Please enter a comment before posting.";
            return RedirectToAction(nameof(Details), new { id = reviewId });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var isCustomer = User.IsInRole("Customer");

        if (isCustomer)
        {
            var canAccess = await _shareService.CanCustomerAccessReviewAsync(userId, reviewId);
            if (!canAccess)
            {
                return RedirectToAction("AccessDenied", "Accounts");
            }
        }

        var role = isCustomer ? "Customer" : "Employee";
        await _commentService.AddCommentAsync(reviewId, userId, body, role);

        TempData["Success"] = "Your comment has been added.";
        return RedirectToAction(nameof(Details), new { id = reviewId });
    }

    [HttpGet]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> CustomerComments(bool awaitingReplyOnly = false)
    {
        var grouped = await _commentService.GetGroupedCustomerCommentsAsync(awaitingReplyOnly);
        var totalCustomerComments = await _commentService.GetTotalCustomerCommentsCountAsync();
        var awaitingCount = await _commentService.GetAwaitingReplyCountAsync();

        var model = new CustomerCommentsViewModel
        {
            TotalCustomerComments = totalCustomerComments,
            AwaitingReplyCount = awaitingCount,
            ReviewsWithCommentsCount = grouped.Count,
            AwaitingReplyOnly = awaitingReplyOnly,
            GroupedComments = grouped
        };

        return View("CustomerComments", model);
    }
}
