using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Notices;

[Authorize(Roles = "Employee")]
public class NoticesController : Controller
{
    private readonly INoticeSearchService _searchService;

    public NoticesController(INoticeSearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var notice = await _searchService.GetNoticeByIdAsync(id);
        if (notice == null)
        {
            return NotFound();
        }

        var associatedReviews = notice.ReviewNotices
            .Select(rn => rn.Review)
            .Distinct()
            .ToList();

        var model = new NoticeDetailsViewModel
        {
            Notice = notice,
            AssociatedReviews = associatedReviews
        };

        return View("NoticeDetails", model);
    }
}
