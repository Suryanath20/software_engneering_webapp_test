using System.ComponentModel.DataAnnotations;
using ANDtr.Web.Models;

namespace ANDtr.Web.Features.Reviews;

public class NoticeNoteInput
{
    public int NoticeId { get; set; }
    public string? Note { get; set; }
}

public class EditReviewViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A title is required.")]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = "Draft";

    public List<ReviewNotice> Notices { get; set; } = new();

    public int Class1Count => Notices.Count(n => n.Notice.RecallClass == "1");
    public int Class2Count => Notices.Count(n => n.Notice.RecallClass == "2");
    public int Class3Count => Notices.Count(n => n.Notice.RecallClass == "3");
}
