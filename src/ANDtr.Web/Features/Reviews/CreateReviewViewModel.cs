using System.ComponentModel.DataAnnotations;
using ANDtr.Web.Models;

namespace ANDtr.Web.Features.Reviews;

public class CreateReviewViewModel
{
    [Required(ErrorMessage = "A title is required before you can save.")]
    [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
    [Display(Name = "Review Title")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Description (optional)")]
    public string? Description { get; set; }

    public List<int> NoticeIds { get; set; } = new();

    public List<RecallNotice> SelectedNotices { get; set; } = new();

    public int Class1Count => SelectedNotices.Count(n => n.RecallClass == "1");
    public int Class2Count => SelectedNotices.Count(n => n.RecallClass == "2");
    public int Class3Count => SelectedNotices.Count(n => n.RecallClass == "3");
}
