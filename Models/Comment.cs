namespace ANDtr.Web.Models;

public class Comment
{
    public int Id { get; set; }

    public int ReviewId { get; set; }
    public virtual RiskReview Review { get; set; } = null!;

    public string AuthorId { get; set; } = string.Empty;
    public virtual ApplicationUser Author { get; set; } = null!;

    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string AuthorRole { get; set; } = "Employee";
}
