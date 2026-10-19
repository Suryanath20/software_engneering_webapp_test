using ANDtr.Web.Models;

namespace ANDtr.Web.Services;

public class CustomerListItemDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ReviewsSharedCount { get; set; }
    public bool IsActive { get; set; }
}

public interface ICustomerService
{
    Task<List<CustomerListItemDto>> GetCustomersAsync(string? search = null);
    Task<ApplicationUser?> GetCustomerByIdAsync(string id);
    Task<(bool Success, string Message, string? GeneratedPassword)> CreateCustomerAsync(string fullName, string email, string? companyName, string? explicitPassword = null);
    Task<bool> ToggleCustomerActiveAsync(string customerId);
    string GenerateSecurePassword();
}
