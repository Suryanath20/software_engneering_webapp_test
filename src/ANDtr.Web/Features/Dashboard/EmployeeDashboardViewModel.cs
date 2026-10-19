using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Dashboard;

public class RecentActivityItem
{
    public string Title { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
    public string IconType { get; set; } = "review"; // "review", "comment", "share", "import"
}

public class EmployeeDashboardViewModel
{
    public string EmployeeName { get; set; } = string.Empty;
    public int TotalNotices { get; set; }
    public int Class1Count { get; set; }
    public int MyReviewsCount { get; set; }
    public int SharedReviewsCount { get; set; }
    public List<RecentActivityItem> RecentActivities { get; set; } = new();
}
