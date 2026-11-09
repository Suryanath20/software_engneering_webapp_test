using ANDtr.Web.Models;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Findings;

public class FindingsViewModel
{
    public RiskReview Review { get; set; } = null!;
    public RecallNotice? ActiveNotice { get; set; }
    public List<CommentDto> Comments { get; set; } = new();
    public int ActiveShareCount { get; set; }
}
