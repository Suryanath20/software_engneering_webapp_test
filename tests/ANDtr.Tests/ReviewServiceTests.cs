using ANDtr.Web.Models;
using ANDtr.Web.Services;
using Xunit;

namespace ANDtr.Tests;

public class ReviewServiceTests
{
    private async Task<(string empId, List<int> noticeIds)> SetupReviewTestData(ANDtr.Web.Data.AppDbContext db)
    {
        var emp = new ApplicationUser
        {
            Id = "emp-review-1",
            UserName = "emp@andtr.test",
            FullName = "Hassan Naveed"
        };
        db.Users.Add(emp);

        var n1 = new RecallNotice { Id = 101, RecallNumber = "Z-0001-2025", ProductCode = "BSZ", FirmName = "Firm A", RecallClass = "1" };
        var n2 = new RecallNotice { Id = 102, RecallNumber = "Z-0002-2025", ProductCode = "BSZ", FirmName = "Firm B", RecallClass = "2" };
        var n3 = new RecallNotice { Id = 103, RecallNumber = "Z-0003-2025", ProductCode = "JOH", FirmName = "Firm C", RecallClass = "1" };
        db.RecallNotices.AddRange(n1, n2, n3);

        await db.SaveChangesAsync();
        return (emp.Id, new List<int> { n1.Id, n2.Id, n3.Id });
    }

    [Fact]
    public async Task CreateReview_WithNotices_SetsDraftStatusAndAssociatesNotices()
    {
        using var db = TestDbContextFactory.Create("Review_Create_Db");
        var (empId, noticeIds) = await SetupReviewTestData(db);
        var service = new ReviewService(db);

        var review = await service.CreateReviewAsync("Ventilator Pressure Review", "Initial assessment of breathing circuits", noticeIds, empId);

        Assert.NotNull(review);
        Assert.Equal("Draft", review.Status);
        Assert.Equal("Ventilator Pressure Review", review.Title);
        Assert.Equal(3, review.ReviewNotices.Count);
    }

    [Fact]
    public async Task UpdateFindings_SanitizesHarmfulHtml_AndPreservesAllowedTags()
    {
        using var db = TestDbContextFactory.Create("Review_Sanitize_Db");
        var (empId, noticeIds) = await SetupReviewTestData(db);
        var service = new ReviewService(db);

        var review = await service.CreateReviewAsync("Findings Test", "Scope", noticeIds, empId);

        var maliciousHtml = "<p>Assessment: <b>Critical alert</b><script>alert('XSS');</script><img src='x' onerror='hack()'/><ul><li>Check line 1</li></ul></p>";
        var updated = await service.UpdateFindingsAsync(review.Id, maliciousHtml, "Published", empId);

        Assert.True(updated);

        var loaded = await service.GetReviewByIdAsync(review.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Published", loaded.Status);
        Assert.DoesNotContain("<script>", loaded.FindingsHtml);
        Assert.DoesNotContain("onerror", loaded.FindingsHtml);
        Assert.Contains("<b>Critical alert</b>", loaded.FindingsHtml);
        Assert.Contains("<ul><li>Check line 1</li></ul>", loaded.FindingsHtml);
    }

    [Fact]
    public async Task UpdateNoticeNote_UpdatesPerNoticeAnalysis()
    {
        using var db = TestDbContextFactory.Create("Review_Note_Db");
        var (empId, noticeIds) = await SetupReviewTestData(db);
        var service = new ReviewService(db);

        var review = await service.CreateReviewAsync("Note Test", "Scope", noticeIds, empId);

        var updated = await service.UpdateNoticeNoteAsync(review.Id, noticeIds[0], "Immediate recall action required for ICU ward.");
        Assert.True(updated);

        var loaded = await service.GetReviewByIdAsync(review.Id);
        var rn = loaded!.ReviewNotices.First(x => x.NoticeId == noticeIds[0]);
        Assert.Equal("Immediate recall action required for ICU ward.", rn.EmployeeNote);
    }

    [Fact]
    public async Task RemoveNoticeFromReview_RemovesNoticeAssociation()
    {
        using var db = TestDbContextFactory.Create("Review_RemoveNotice_Db");
        var (empId, noticeIds) = await SetupReviewTestData(db);
        var service = new ReviewService(db);

        var review = await service.CreateReviewAsync("Remove Notice Test", "Scope", noticeIds, empId);
        Assert.Equal(3, review.ReviewNotices.Count);

        var removed = await service.RemoveNoticeFromReviewAsync(review.Id, noticeIds[1]);
        Assert.True(removed);

        var loaded = await service.GetReviewByIdAsync(review.Id);
        Assert.Equal(2, loaded!.ReviewNotices.Count);
        Assert.DoesNotContain(loaded.ReviewNotices, x => x.NoticeId == noticeIds[1]);
    }

    [Fact]
    public async Task DeleteReview_RemovesReviewAndItsAssociations()
    {
        using var db = TestDbContextFactory.Create("Review_Delete_Db");
        var (empId, noticeIds) = await SetupReviewTestData(db);
        var service = new ReviewService(db);

        var review = await service.CreateReviewAsync("To Delete", "Scope", noticeIds, empId);

        var deleted = await service.DeleteReviewAsync(review.Id, empId);
        Assert.True(deleted);

        var loaded = await service.GetReviewByIdAsync(review.Id);
        Assert.Null(loaded);
    }
}
