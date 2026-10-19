using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Data;
using ANDtr.Web.Models;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Sharing;

[Authorize(Roles = "Employee")]
public class SharingController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IShareService _shareService;
    private readonly IReviewService _reviewService;

    public SharingController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IShareService shareService,
        IReviewService reviewService)
    {
        _db = db;
        _userManager = userManager;
        _shareService = shareService;
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? reviewId = null)
    {
        var reviews = await _db.RiskReviews
            .Include(r => r.ReviewNotices)
            .Include(r => r.Shares)
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync();

        var customers = (await _userManager.GetUsersInRoleAsync("Customer"))
            .Where(c => c.IsActive)
            .OrderBy(c => c.FullName)
            .ToList();

        var activeReviewId = reviewId ?? reviews.FirstOrDefault()?.Id;
        var selectedReview = reviews.FirstOrDefault(r => r.Id == activeReviewId);

        var currentShares = await _shareService.GetActiveSharesAsync(activeReviewId);

        var model = new ShareViewModel
        {
            SelectedReviewId = activeReviewId,
            AvailableReviews = reviews,
            AvailableCustomers = customers,
            SelectedReview = selectedReview,
            CurrentShares = currentShares
        };

        return View("Sharing", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Share(int reviewId, string customerId, string? message)
    {
        var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        if (string.IsNullOrWhiteSpace(customerId))
        {
            TempData["Error"] = "Select a customer to share with.";
            return RedirectToAction(nameof(Index), new { reviewId });
        }

        var result = await _shareService.ShareReviewAsync(reviewId, customerId, employeeId, message);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction(nameof(Index), new { reviewId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StopSharing(int shareId, int? reviewId = null)
    {
        var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var stopped = await _shareService.StopSharingAsync(shareId, employeeId);

        if (stopped)
        {
            TempData["Success"] = "Sharing has been stopped.";
        }
        else
        {
            TempData["Error"] = "Could not revoke sharing or sharing already revoked.";
        }

        return RedirectToAction(nameof(Index), new { reviewId });
    }
}
