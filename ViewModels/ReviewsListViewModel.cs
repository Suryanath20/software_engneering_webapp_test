using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Reviews;

public class ReviewsListViewModel
{
    public string? Search { get; set; }
    public string? SharingFilter { get; set; } = "All reviews";
    public List<ReviewDto> Reviews { get; set; } = new();
}
