using System.Text.RegularExpressions;
using ANDtr.Web.Models;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Search;

public class SearchResultsViewModel
{
    public NoticeSearchCriteria Criteria { get; set; } = new();
    public PagedResult<RecallNotice> Results { get; set; } = new();

    /// <summary>Helper method to safely highlight keywords within text using &lt;mark&gt; tags</summary>
    public static string Highlight(string? text, string? keyword, int maxLength = 200)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return text.Length > maxLength ? text.Substring(0, maxLength) + "..." : text;
        }

        var escaped = System.Net.WebUtility.HtmlEncode(text);
        var pattern = Regex.Escape(keyword.Trim());

        // Find match position to center snippet if long
        var match = Regex.Match(escaped, pattern, RegexOptions.IgnoreCase);
        if (match.Success && escaped.Length > maxLength)
        {
            var start = Math.Max(0, match.Index - 40);
            var length = Math.Min(escaped.Length - start, maxLength);
            escaped = (start > 0 ? "..." : "") + escaped.Substring(start, length) + (start + length < escaped.Length ? "..." : "");
        }
        else if (escaped.Length > maxLength)
        {
            escaped = escaped.Substring(0, maxLength) + "...";
        }

        return Regex.Replace(escaped, pattern, "<mark>$0</mark>", RegexOptions.IgnoreCase);
    }
}
