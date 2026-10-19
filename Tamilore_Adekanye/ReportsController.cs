using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Data;
using ANDtr.Web.Models;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Reports;

[Authorize]
public class ReportsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IReportService _reportService;
    private readonly IShareService _shareService;

    public ReportsController(
        AppDbContext db,
        IReportService reportService,
        IShareService shareService)
    {
        _db = db;
        _reportService = reportService;
        _shareService = shareService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? reviewId = null)
    {
        var isCustomer = User.IsInRole("Customer");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        List<RiskReview> availableReviews;

        if (isCustomer)
        {
            // Customer can only choose from reviews actively shared with them
            availableReviews = await _db.ReviewShares
                .Where(s => s.CustomerId == userId && s.RevokedAt == null)
                .Select(s => s.Review)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();
        }
        else
        {
            availableReviews = await _db.RiskReviews
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();
        }

        var activeReviewId = reviewId ?? availableReviews.FirstOrDefault()?.Id;

        // Security check
        if (activeReviewId.HasValue && isCustomer)
        {
            var canAccess = await _shareService.CanCustomerAccessReviewAsync(userId, activeReviewId.Value);
            if (!canAccess)
            {
                return RedirectToAction("AccessDenied", "Accounts");
            }
        }

        RiskReviewReportDto? reportData = null;
        if (activeReviewId.HasValue)
        {
            reportData = await _reportService.GetReportDataAsync(activeReviewId.Value);
        }

        var model = new ReportViewModel
        {
            SelectedReviewId = activeReviewId,
            AvailableReviews = availableReviews,
            ReportData = reportData
        };

        return View("Report", model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(int reviewId)
    {
        var isCustomer = User.IsInRole("Customer");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        if (isCustomer)
        {
            var canAccess = await _shareService.CanCustomerAccessReviewAsync(userId, reviewId);
            if (!canAccess)
            {
                return RedirectToAction("AccessDenied", "Accounts");
            }
        }

        var bytes = await _reportService.GenerateCsvExportAsync(reviewId);
        if (bytes.Length == 0)
        {
            return NotFound();
        }

        return File(bytes, "text/csv", $"ANDtr_RiskReview_{reviewId}_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
