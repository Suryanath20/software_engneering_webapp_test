namespace ANDtr.Web.Models;

/// <summary>
/// Represents a company Risk Review grouping safety notices with findings and assessment.
/// </summary>
public class RiskReview
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    /// <summary>Rich text findings authored by employees (sanitized HTML)</summary>
    public string? FindingsHtml { get; set; }

    /// <summary>Review status: "Draft" or "Published"</summary>
    public string Status { get; set; } = "Draft";

    public string CreatedById { get; set; } = string.Empty;
    public virtual ApplicationUser? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<ReviewNotice> ReviewNotices { get; set; } = new List<ReviewNotice>();
    public virtual ICollection<ReviewShare> Shares { get; set; } = new List<ReviewShare>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
