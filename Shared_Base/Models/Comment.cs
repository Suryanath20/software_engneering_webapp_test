namespace ANDtr.Web.Models;

/// <summary>
/// Comments on a Risk Review authored by either an Employee or Customer.
/// </summary>
public class Comment
{
    public int Id { get; set; }

    public int ReviewId { get; set; }
    public virtual RiskReview Review { get; set; } = null!;

    public string AuthorId { get; set; } = string.Empty;
    public virtual ApplicationUser Author { get; set; } = null!;

    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Author's role at time of posting: "Employee" or "Customer"</summary>
    public string AuthorRole { get; set; } = "Employee";
}
