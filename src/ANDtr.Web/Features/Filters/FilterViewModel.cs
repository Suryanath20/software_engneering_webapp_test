using ANDtr.Web.Models;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Filters;

public class FilterViewModel
{
    public string? Keyword { get; set; } = "leak";
    public List<string> Classes { get; set; } = new() { "1" };
    public string? FirmName { get; set; }
    public string? ProductCode { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string SortBy { get; set; } = "date_desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public List<string> AvailableFirms { get; set; } = new();
    public List<string> AvailableProductCodes { get; set; } = new();

    public int Class1TotalCount { get; set; }
    public int Class2TotalCount { get; set; }
    public int Class3TotalCount { get; set; }

    public PagedResult<RecallNotice> Results { get; set; } = new();
}
