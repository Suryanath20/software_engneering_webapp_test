using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Customers;

[Authorize(Roles = "Employee")]
public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search = null)
    {
        var customers = await _customerService.GetCustomersAsync(search);

        var generatedPassword = _customerService.GenerateSecurePassword();
        var model = new CustomerListViewModel
        {
            Search = search,
            Customers = customers,
            NewUser = new CreateCustomerViewModel
            {
                Password = generatedPassword,
                ConfirmPassword = generatedPassword,
                MustChangePassword = true
            }
        };

        return View("Views/Index", model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var generatedPassword = _customerService.GenerateSecurePassword();
        var model = new CreateCustomerViewModel
        {
            Password = generatedPassword,
            ConfirmPassword = generatedPassword,
            MustChangePassword = true
        };

        return View("Views/Create", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerViewModel model, string? returnView = null)
    {
        if (!ModelState.IsValid)
        {
            if (string.Equals(returnView, "Index", StringComparison.OrdinalIgnoreCase))
            {
                var customers = await _customerService.GetCustomersAsync();
                var listModel = new CustomerListViewModel
                {
                    Customers = customers,
                    NewUser = model
                };
                return View("Views/Index", listModel);
            }
            return View("Views/Create", model);
        }

        var result = await _customerService.CreateCustomerAsync(
            model.FullName, model.Email, model.Company, model.Password, model.Role, model.MustChangePassword);

        if (!result.Success)
        {
            ModelState.AddModelError("Email", result.Message);
            if (string.Equals(returnView, "Index", StringComparison.OrdinalIgnoreCase))
            {
                var customers = await _customerService.GetCustomersAsync();
                var listModel = new CustomerListViewModel
                {
                    Customers = customers,
                    NewUser = model
                };
                return View("Views/Index", listModel);
            }
            return View("Views/Create", model);
        }

        TempData["Success"] = $"Account created for {model.FullName} ({model.Email}) as {model.Role}. Temporary password: {result.GeneratedPassword}";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string customerId)
    {
        var toggled = await _customerService.ToggleCustomerActiveAsync(customerId);
        if (toggled)
        {
            TempData["Success"] = "Customer account status updated.";
        }
        else
        {
            TempData["Error"] = "Could not update customer status.";
        }

        return RedirectToAction(nameof(Index));
    }
}
