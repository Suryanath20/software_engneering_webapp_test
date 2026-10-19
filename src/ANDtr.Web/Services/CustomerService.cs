using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Data;
using ANDtr.Web.Models;

namespace ANDtr.Web.Services;

public class CustomerService : ICustomerService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;

    public CustomerService(UserManager<ApplicationUser> userManager, AppDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<List<CustomerListItemDto>> GetCustomersAsync(string? search = null)
    {
        var customers = await _userManager.GetUsersInRoleAsync("Customer");
        var customerIds = customers.Select(c => c.Id).ToList();

        var shareCounts = await _db.ReviewShares
            .Where(s => customerIds.Contains(s.CustomerId) && s.RevokedAt == null)
            .GroupBy(s => s.CustomerId)
            .Select(g => new { CustomerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.CustomerId, g => g.Count);

        var list = customers.Select(c => new CustomerListItemDto
        {
            Id = c.Id,
            FullName = c.FullName,
            Email = c.Email ?? string.Empty,
            CompanyName = c.CompanyName ?? string.Empty,
            CreatedAt = c.CreatedAt,
            ReviewsSharedCount = shareCounts.GetValueOrDefault(c.Id, 0),
            IsActive = c.IsActive
        });

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            list = list.Where(c =>
                c.FullName.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                c.CompanyName.Contains(s, StringComparison.OrdinalIgnoreCase)
            );
        }

        return list.OrderByDescending(c => c.CreatedAt).ToList();
    }

    public async Task<ApplicationUser?> GetCustomerByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null && await _userManager.IsInRoleAsync(user, "Customer"))
        {
            return user;
        }
        return null;
    }

    public async Task<(bool Success, string Message, string? GeneratedPassword)> CreateCustomerAsync(
        string fullName, string email, string? companyName, string? explicitPassword = null, string role = "Customer", bool mustChangePassword = true)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing != null)
        {
            return (false, "An account with this email address already exists.", null);
        }

        var password = string.IsNullOrWhiteSpace(explicitPassword) ? GenerateSecurePassword() : explicitPassword;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName.Trim(),
            CompanyName = string.IsNullOrWhiteSpace(companyName) ? null : companyName.Trim(),
            IsActive = true,
            MustChangePassword = mustChangePassword,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var error = string.Join("; ", result.Errors.Select(e => e.Description));
            return (false, $"Could not create user: {error}", null);
        }

        var targetRole = string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase) ? "Employee" : "Customer";
        await _userManager.AddToRoleAsync(user, targetRole);
        return (true, $"{targetRole} account created successfully.", password);
    }

    public async Task<bool> ToggleCustomerActiveAsync(string customerId)
    {
        var user = await _userManager.FindByIdAsync(customerId);
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        return true;
    }

    public string GenerateSecurePassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%^&*";

        var chars = new char[10];
        chars[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
        chars[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
        chars[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        chars[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

        var all = upper + lower + digits + special;
        for (int i = 4; i < 10; i++)
        {
            chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
        }

        // Shuffle
        return new string(chars.OrderBy(_ => RandomNumberGenerator.GetInt32(100)).ToArray());
    }
}
