using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Search;

[Authorize(Roles = "Employee")]
public class SearchController : Controller
{
    private readonly INoticeSearchService _searchService;

    public SearchController(INoticeSearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var model = new SearchViewModel();
        return View("Search", model);
    }

    [HttpGet]
    public async Task<IActionResult> Results(
        string? keyword,
        bool searchInDescription = true,
        bool searchInTradeName = true,
        bool searchInFirmName = true,
        bool searchInRecallReason = true,
        bool matchPartialWords = true,
        bool includeRelatedTerms = false,
        int page = 1,
        int pageSize = 20)
    {
        var criteria = new NoticeSearchCriteria
        {
            Keyword = keyword,
            SearchInDescription = searchInDescription,
            SearchInTradeName = searchInTradeName,
            SearchInFirmName = searchInFirmName,
            SearchInRecallReason = searchInRecallReason,
            MatchPartialWords = matchPartialWords,
            IncludeRelatedTerms = includeRelatedTerms,
            PageNumber = page,
            PageSize = pageSize,
            SortBy = "date_desc"
        };

        var results = await _searchService.SearchNoticesAsync(criteria);

        var model = new SearchResultsViewModel
        {
            Criteria = criteria,
            Results = results
        };

        return View("SearchResults", model);
    }
}
