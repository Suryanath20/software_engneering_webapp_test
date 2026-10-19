using Microsoft.AspNetCore.Identity;

namespace ANDtr.Web.Models;

/// <summary>
/// Application user extending IdentityUser with display name, company affiliation and active status.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public string? CompanyName { get; set; }

    public bool IsActive { get; set; } = true;

    public bool MustChangePassword { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<RiskReview> CreatedReviews { get; set; } = new List<RiskReview>();
    public virtual ICollection<ReviewShare> ReceivedShares { get; set; } = new List<ReviewShare>();
    public virtual ICollection<ReviewShare> InitiatedShares { get; set; } = new List<ReviewShare>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
