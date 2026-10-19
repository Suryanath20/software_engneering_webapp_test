namespace ANDtr.Web.Models;

/// <summary>
/// Many-to-many join entity between RiskReview and RecallNotice with per-notice employee notes.
/// </summary>
public class ReviewNotice
{
    public int ReviewId { get; set; }
    public virtual RiskReview Review { get; set; } = null!;

    public int NoticeId { get; set; }
    public virtual RecallNotice Notice { get; set; } = null!;

    /// <summary>Specific comment or risk analysis note attached to this notice within the review.</summary>
    public string? EmployeeNote { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
