namespace ANDtr.Web.Features.Search;

public class SearchViewModel
{
    public string? Keyword { get; set; } = "leak";
    public bool SearchInDescription { get; set; } = true;
    public bool SearchInTradeName { get; set; } = true;
    public bool SearchInFirmName { get; set; } = true;
    public bool SearchInRecallReason { get; set; } = true;
    public bool MatchPartialWords { get; set; } = true;
    public bool IncludeRelatedTerms { get; set; } = false;

    public List<string> RecentKeywords { get; set; } = new() { "leak", "software", "tracheostomy", "battery" };
}
