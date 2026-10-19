using System.ComponentModel.DataAnnotations;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Customers;

public class CustomerListViewModel
{
    public string? Search { get; set; }
    public List<CustomerListItemDto> Customers { get; set; } = new();
}

public class CreateCustomerViewModel
{
    [Required(ErrorMessage = "Full Name is required.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Company")]
    public string? Company { get; set; }

    public string Role { get; set; } = "Customer";

    [Required(ErrorMessage = "A temporary password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Temporary Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the password.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
