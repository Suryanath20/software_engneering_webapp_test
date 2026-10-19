using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Data;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Filters;

[Authorize(Roles = "Employee")]
public class FiltersController : Controller
{
    private readonly INoticeSearchService _searchService;
    private readonly AppDbContext _db;

    public FiltersController(INoticeSearchService searchService, AppDbContext db)
    {
        _searchService = searchService;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] FilterViewModel model)
    {
        model.Classes ??= new List<string>();

        var criteria = new NoticeSearchCriteria
        {
            Keyword = model.Keyword,
            Classes = model.Classes,
            FirmName = model.FirmName,
            ProductCode = model.ProductCode,
            FromDate = model.FromDate,
            ToDate = model.ToDate,
            SortBy = model.SortBy,
            PageNumber = model.Page,
            PageSize = model.PageSize
        };

        model.Results = await _searchService.SearchNoticesAsync(criteria);
        model.AvailableFirms = await _searchService.GetAllFirmsAsync();
        model.AvailableProductCodes = await _searchService.GetAllProductCodesAsync();

        model.Class1TotalCount = await _db.RecallNotices.CountAsync(n => n.RecallClass == "1");
        model.Class2TotalCount = await _db.RecallNotices.CountAsync(n => n.RecallClass == "2");
        model.Class3TotalCount = await _db.RecallNotices.CountAsync(n => n.RecallClass == "3");

        return View("Views/Index", model);
    }
}
