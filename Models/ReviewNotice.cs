namespace ANDtr.Web.Models;

public class ReviewNotice
{
    public int ReviewId { get; set; }
    public virtual RiskReview Review { get; set; } = null!;

    public int NoticeId { get; set; }
    public virtual RecallNotice Notice { get; set; } = null!;

    public string? EmployeeNote { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
