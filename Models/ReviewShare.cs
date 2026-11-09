namespace ANDtr.Web.Models;

public class ReviewShare
{
    public int Id { get; set; }

    public int ReviewId { get; set; }
    public virtual RiskReview Review { get; set; } = null!;

    public string CustomerId { get; set; } = string.Empty;
    public virtual ApplicationUser Customer { get; set; } = null!;

    public string SharedById { get; set; } = string.Empty;
    public virtual ApplicationUser SharedBy { get; set; } = null!;

    public DateTime SharedAt { get; set; } = DateTime.UtcNow;
    public string? Message { get; set; }
    public DateTime? RevokedAt { get; set; }

    public bool IsActive => RevokedAt == null;
}
