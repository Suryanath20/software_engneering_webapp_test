using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Models;

namespace ANDtr.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        // 1. Roles
        string[] roles = { "Employee", "Customer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Initial Base Users
        var usersToSeed = new[]
        {
            (Email: "employee@andtr.test", Password: "Employee#123", Name: "Alex Taylor", Company: "ANDtr Risk Team", Role: "Employee"),
            (Email: "customer@andtr.test", Password: "Customer#123", Name: "Jordan Smith", Company: "Northfield Medical Group", Role: "Customer")
        };

        foreach (var u in usersToSeed)
        {
            var existing = await userManager.FindByEmailAsync(u.Email);
            if (existing == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = u.Email,
                    Email = u.Email,
                    FullName = u.Name,
                    CompanyName = u.Company,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                };

                var createRes = await userManager.CreateAsync(newUser, u.Password);
                if (createRes.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, u.Role);
                }
            }
        }

        logger.LogInformation("[ANDtr] Initial identity roles and users seeded successfully.");
    }
}
