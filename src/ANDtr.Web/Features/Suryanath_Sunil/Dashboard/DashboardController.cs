using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Data;
using ANDtr.Web.Models;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Dashboard;

[Authorize]
public class DashboardController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IReviewService _reviewService;
    private readonly IShareService _shareService;

    public DashboardController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IReviewService reviewService,
        IShareService shareService)
    {
        _db = db;
        _userManager = userManager;
        _reviewService = reviewService;
        _shareService = shareService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Customer"))
        {
            return await Customer();
        }

        return await Employee();
    }

    [HttpGet]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> Employee()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var user = await _userManager.FindByIdAsync(userId);

        var totalNotices = await _db.RecallNotices.CountAsync();
        var class1Count = await _db.RecallNotices.CountAsync(n => n.RecallClass == "1");
        var myReviewsCount = await _reviewService.GetEmployeeReviewCountAsync(userId);
        var sharedReviewsCount = await _reviewService.GetSharedReviewCountAsync();

        // Dynamic recent activities fetched directly from database
        var recentActivities = new List<RecentActivityItem>();

        var latestReview = await _db.RiskReviews.OrderByDescending(r => r.UpdatedAt).FirstOrDefaultAsync();
        if (latestReview != null)
        {
            recentActivities.Add(new RecentActivityItem
            {
                Title = $"Risk review \"{latestReview.Title}\" updated",
                TimeAgo = "Recent",
                IconType = "review"
            });
        }

        var latestComment = await _db.Comments.Include(c => c.Author).Include(c => c.Review).OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync();
        if (latestComment != null)
        {
            recentActivities.Add(new RecentActivityItem
            {
                Title = $"{latestComment.Author?.FullName ?? "User"} commented on \"{latestComment.Review?.Title ?? "Risk Review"}\"",
                TimeAgo = "Recent",
                IconType = "comment"
            });
        }

        var latestShare = await _db.ReviewShares.Include(s => s.Customer).Include(s => s.Review).OrderByDescending(s => s.SharedAt).FirstOrDefaultAsync();
        if (latestShare != null)
        {
            var targetOrg = !string.IsNullOrEmpty(latestShare.Customer?.CompanyName) ? latestShare.Customer.CompanyName : (latestShare.Customer?.FullName ?? "Customer");
            recentActivities.Add(new RecentActivityItem
            {
                Title = $"Shared \"{latestShare.Review?.Title ?? "Review"}\" with {targetOrg}",
                TimeAgo = "Recent",
                IconType = "share"
            });
        }

        recentActivities.Add(new RecentActivityItem
        {
            Title = $"Dataset active: {totalNotices} FDA notices indexed",
            TimeAgo = "Verified",
            IconType = "import"
        });

        var model = new EmployeeDashboardViewModel
        {
            EmployeeName = user?.FullName ?? "Employee",
            TotalNotices = totalNotices,
            Class1Count = class1Count,
            MyReviewsCount = myReviewsCount,
            SharedReviewsCount = sharedReviewsCount,
            RecentActivities = recentActivities
        };

        return View("Views/EmployeeDashboard", model);
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Customer()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var user = await _userManager.FindByIdAsync(userId);

        var sharedReviews = await _shareService.GetSharedReviewsForCustomerAsync(userId);
        var commentCount = await _db.Comments.CountAsync(c => c.AuthorId == userId);
        var newCount = sharedReviews.Count(r => r.Status == "New");

        var model = new CustomerDashboardViewModel
        {
            CustomerName = user?.FullName ?? "Customer",
            CompanyName = user?.CompanyName ?? "Partner Hospital",
            SharedReviewsCount = sharedReviews.Count,
            NewSinceLastVisitCount = newCount,
            CustomerCommentsCount = commentCount,
            SharedReviews = sharedReviews
        };

        return View("Views/CustomerDashboard", model);
    }
}
