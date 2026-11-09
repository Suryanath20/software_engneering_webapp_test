using ANDtr.Web.Models;
using ANDtr.Web.Services;
using Xunit;

namespace ANDtr.Tests;

public class ShareServiceTests
{
    private async Task<(string empId, string cust1Id, string cust2Id, int reviewId)> SetupBaseData(ANDtr.Web.Data.AppDbContext db)
    {
        var employee = new ApplicationUser
        {
            Id = "emp-1",
            UserName = "emp@andtr.test",
            FullName = "Suryanath Sunil",
            IsActive = true
        };

        var cust1 = new ApplicationUser
        {
            Id = "cust-1",
            UserName = "cust1@client.test",
            FullName = "Maya Patel",
            CompanyName = "Northfield Medical",
            IsActive = true
        };

        var cust2 = new ApplicationUser
        {
            Id = "cust-2",
            UserName = "cust2@client.test",
            FullName = "Daniel Okafor",
            CompanyName = "Westbridge Hospital",
            IsActive = true
        };

        var review = new RiskReview
        {
            Id = 10,
            Title = "Anesthesia gas machine leak review",
            CreatedById = employee.Id,
            Status = "Published"
        };

        db.Users.AddRange(employee, cust1, cust2);
        db.RiskReviews.Add(review);
        await db.SaveChangesAsync();

        return (employee.Id, cust1.Id, cust2.Id, review.Id);
    }

    [Fact]
    public async Task Customer_CannotAccess_UnsharedReview()
    {
        using var db = TestDbContextFactory.Create("Share_Security_Db1");
        var (_, cust1Id, _, reviewId) = await SetupBaseData(db);
        var service = new ShareService(db);

        // Not shared yet
        var canAccess = await service.CanCustomerAccessReviewAsync(cust1Id, reviewId);

        Assert.False(canAccess, "Security violation: Customer was able to access an unshared review!");
    }

    [Fact]
    public async Task Customer_CanAccess_ActivelySharedReview()
    {
        using var db = TestDbContextFactory.Create("Share_Security_Db2");
        var (empId, cust1Id, _, reviewId) = await SetupBaseData(db);
        var service = new ShareService(db);

        // Share review with customer 1
        var (success, _) = await service.ShareReviewAsync(reviewId, cust1Id, empId, "Please review.");
        Assert.True(success);

        // Customer 1 should now have access
        var canAccessCust1 = await service.CanCustomerAccessReviewAsync(cust1Id, reviewId);
        Assert.True(canAccessCust1, "Customer 1 should have access to the shared review.");
    }

    [Fact]
    public async Task Customer_CannotAccess_ReviewSharedWithOtherCustomer()
    {
        using var db = TestDbContextFactory.Create("Share_Security_Db3");
        var (empId, cust1Id, cust2Id, reviewId) = await SetupBaseData(db);
        var service = new ShareService(db);

        // Share review with customer 1 ONLY
        await service.ShareReviewAsync(reviewId, cust1Id, empId);

        // Customer 2 should NOT have access
        var canAccessCust2 = await service.CanCustomerAccessReviewAsync(cust2Id, reviewId);
        Assert.False(canAccessCust2, "Security violation: Customer 2 accessed review shared only with Customer 1!");
    }

    [Fact]
    public async Task StopSharing_RevokesCustomerAccess()
    {
        using var db = TestDbContextFactory.Create("Share_Revoke_Db");
        var (empId, cust1Id, _, reviewId) = await SetupBaseData(db);
        var service = new ShareService(db);

        // 1. Share
        await service.ShareReviewAsync(reviewId, cust1Id, empId);
        var activeShares = await service.GetActiveSharesAsync(reviewId);
        var shareId = activeShares[0].ShareId;

        // Verify access granted
        Assert.True(await service.CanCustomerAccessReviewAsync(cust1Id, reviewId));

        // 2. Stop sharing
        var stopped = await service.StopSharingAsync(shareId, empId);
        Assert.True(stopped);

        // 3. Verify access revoked
        var canAccessAfterRevoke = await service.CanCustomerAccessReviewAsync(cust1Id, reviewId);
        Assert.False(canAccessAfterRevoke, "Customer should no longer have access after sharing is revoked.");
    }

    [Fact]
    public async Task ShareReview_PreventsDuplicateActiveShare()
    {
        using var db = TestDbContextFactory.Create("Share_Duplicate_Db");
        var (empId, cust1Id, _, reviewId) = await SetupBaseData(db);
        var service = new ShareService(db);

        var (success1, _) = await service.ShareReviewAsync(reviewId, cust1Id, empId);
        Assert.True(success1);

        // Second attempt to share the same review to the same customer
        var (success2, msg2) = await service.ShareReviewAsync(reviewId, cust1Id, empId);
        Assert.False(success2);
        Assert.Contains("already actively shared", msg2);
    }
}
