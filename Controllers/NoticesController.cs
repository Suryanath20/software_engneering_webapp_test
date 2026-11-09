using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Notices;

[Authorize]
public class NoticesController : Controller
{
    private readonly INoticeSearchService _searchService;
    private readonly IShareService _shareService;

    public NoticesController(INoticeSearchService searchService, IShareService shareService)
    {
        _searchService = searchService;
        _shareService = shareService;
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var notice = await _searchService.GetNoticeByIdAsync(id);
        if (notice == null)
        {
            return NotFound();
        }

        var isCustomer = User.IsInRole("Customer");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        var associatedReviews = notice.ReviewNotices
            .Select(rn => rn.Review)
            .Distinct()
            .ToList();

        // If customer, only show associated reviews that are actively shared with them
        if (isCustomer)
        {
            var accessibleReviews = new List<Models.RiskReview>();
            foreach (var rev in associatedReviews)
            {
                if (await _shareService.CanCustomerAccessReviewAsync(userId, rev.Id))
                {
                    accessibleReviews.Add(rev);
                }
            }
            associatedReviews = accessibleReviews;
        }

        var model = new NoticeDetailsViewModel
        {
            Notice = notice,
            AssociatedReviews = associatedReviews
        };

        return View("NoticeDetails", model);
    }
}
