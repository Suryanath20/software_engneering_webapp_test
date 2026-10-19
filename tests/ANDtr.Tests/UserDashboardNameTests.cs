using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ANDtr.Web.Common;
using ANDtr.Web.Data;
using ANDtr.Web.Features.Dashboard;
using ANDtr.Web.Models;
using ANDtr.Web.Services;
using Xunit;

namespace ANDtr.Tests;

public class UserDashboardNameTests
{
    private static IServiceProvider CreateServices(string dbName)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase(dbName));
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IShareService, ShareService>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task TwoDifferentUsers_SeeTheirOwnRespectiveNamesAndRoles()
    {
        var sp = CreateServices("UserDashboard_TwoUsers_Db");
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var claimsFactory = sp.GetRequiredService<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var db = sp.GetRequiredService<AppDbContext>();
        var reviewService = sp.GetRequiredService<IReviewService>();
        var shareService = sp.GetRequiredService<IShareService>();

        // 1. Ensure Roles exist
        await roleManager.CreateAsync(new IdentityRole("Employee"));
        await roleManager.CreateAsync(new IdentityRole("Customer"));

        // 2. Create User A (Employee: Dr. Marcus Vance)
        var userA = new ApplicationUser
        {
            UserName = "m.vance@andtr.test",
            Email = "m.vance@andtr.test",
            FullName = "Dr. Marcus Vance",
            CompanyName = "ANDtr Clinical Engineering",
            IsActive = true
        };
        var createARes = await userManager.CreateAsync(userA, "Pass123!");
        Assert.True(createARes.Succeeded);
        await userManager.AddToRoleAsync(userA, "Employee");

        // 3. Create User B (Customer: Sarah Jenkins)
        var userB = new ApplicationUser
        {
            UserName = "s.jenkins@clevelandclinic.test",
            Email = "s.jenkins@clevelandclinic.test",
            FullName = "Sarah Jenkins",
            CompanyName = "Cleveland Clinic Procurement",
            IsActive = true
        };
        var createBRes = await userManager.CreateAsync(userB, "Pass123!");
        Assert.True(createBRes.Succeeded);
        await userManager.AddToRoleAsync(userB, "Customer");

        // 4. Verify Claims generation for User A
        var principalA = await claimsFactory.CreateAsync(userA);
        var fullNameClaimA = principalA.FindFirst("FullName")?.Value;
        var roleClaimA = principalA.FindFirst("RoleLabel")?.Value;

        Assert.Equal("Dr. Marcus Vance", fullNameClaimA);
        Assert.Equal("Employee", roleClaimA);
        Assert.NotEqual("Sarah Jenkins", fullNameClaimA);

        // 5. Verify Claims generation for User B
        var principalB = await claimsFactory.CreateAsync(userB);
        var fullNameClaimB = principalB.FindFirst("FullName")?.Value;
        var roleClaimB = principalB.FindFirst("RoleLabel")?.Value;

        Assert.Equal("Sarah Jenkins", fullNameClaimB);
        Assert.Equal("Customer", roleClaimB);
        Assert.NotEqual("Dr. Marcus Vance", fullNameClaimB);

        // 6. Verify Dashboard Controller displays each user's own name
        var controller = new DashboardController(db, userManager, reviewService, shareService);

        // Act for User A (Employee)
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principalA }
        };
        var resultA = await controller.Employee() as ViewResult;
        Assert.NotNull(resultA);
        var modelA = Assert.IsType<EmployeeDashboardViewModel>(resultA.Model);
        Assert.Equal("Dr. Marcus Vance", modelA.EmployeeName);
        Assert.NotEqual("Sarah Jenkins", modelA.EmployeeName);

        // Act for User B (Customer)
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principalB }
        };
        var resultB = await controller.Customer() as ViewResult;
        Assert.NotNull(resultB);
        var modelB = Assert.IsType<CustomerDashboardViewModel>(resultB.Model);
        Assert.Equal("Sarah Jenkins", modelB.CustomerName);
        Assert.Equal("Cleveland Clinic Procurement", modelB.CompanyName);
        Assert.NotEqual("Dr. Marcus Vance", modelB.CustomerName);
    }

    [Fact]
    public async Task AddUser_ValidatesDuplicateEmail_AndEnforcesMustChangePassword()
    {
        var sp = CreateServices("UserDashboard_AddUserValidation_Db");
        var customerService = sp.GetRequiredService<ICustomerService>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

        await roleManager.CreateAsync(new IdentityRole("Customer"));
        await roleManager.CreateAsync(new IdentityRole("Employee"));

        // 1. Create first user with email
        var (success1, _, _) = await customerService.CreateCustomerAsync(
            "Original User",
            "duplicate.test@andtr.test",
            "Test Hospital",
            "Temp#12345",
            "Customer",
            mustChangePassword: true);

        Assert.True(success1);

        // Verify MustChangePassword flag was set
        var createdUser = await userManager.FindByEmailAsync("duplicate.test@andtr.test");
        Assert.NotNull(createdUser);
        Assert.True(createdUser.MustChangePassword);
        Assert.Equal("Original User", createdUser.FullName);

        // 2. Attempt creating duplicate email
        var (success2, message2, _) = await customerService.CreateCustomerAsync(
            "Second User",
            "duplicate.test@andtr.test",
            "Another Hospital",
            "Temp#99999",
            "Employee",
            mustChangePassword: true);

        Assert.False(success2);
        Assert.Contains("already exists", message2, StringComparison.OrdinalIgnoreCase);
    }
}
