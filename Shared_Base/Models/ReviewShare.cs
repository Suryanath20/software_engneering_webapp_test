namespace ANDtr.Web.Models;

/// <summary>
/// Represents granting customer access to a specific published Risk Review.
/// </summary>
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

    /// <summary>Optional message included when sharing the review.</summary>
    public string? Message { get; set; }

    /// <summary>Timestamp when sharing was revoked/stopped; null indicates actively shared.</summary>
    public DateTime? RevokedAt { get; set; }

    public bool IsActive => RevokedAt == null;
}
