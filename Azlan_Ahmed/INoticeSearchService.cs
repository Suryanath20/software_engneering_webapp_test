using ANDtr.Web.Models;

namespace ANDtr.Web.Services;

public class NoticeSearchCriteria
{
    public string? Keyword { get; set; }
    public bool SearchInDescription { get; set; } = true;
    public bool SearchInTradeName { get; set; } = true;
    public bool SearchInFirmName { get; set; } = true;
    public bool SearchInRecallReason { get; set; } = true;

    /// <summary>Matches substrings within words (e.g. 'tracheo' matches 'tracheostomy')</summary>
    public bool MatchPartialWords { get; set; } = true;

    /// <summary>Expands search with biological / medical synonym stems</summary>
    public bool IncludeRelatedTerms { get; set; } = false;

    public List<string> Classes { get; set; } = new();
    public string? FirmName { get; set; }
    public string? ProductCode { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public string SortBy { get; set; } = "date_desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public interface INoticeSearchService
{
    Task<PagedResult<RecallNotice>> SearchNoticesAsync(NoticeSearchCriteria criteria);
    Task<List<string>> GetAllFirmsAsync();
    Task<List<string>> GetAllProductCodesAsync();
    Task<RecallNotice?> GetNoticeByIdAsync(int id);
    Task<RecallNotice?> GetNoticeByRecallNumberAsync(string recallNumber);
    Task<List<RecallNotice>> GetNoticesByIdsAsync(IEnumerable<int> ids);
}
