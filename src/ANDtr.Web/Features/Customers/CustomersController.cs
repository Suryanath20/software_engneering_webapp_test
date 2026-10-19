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

        var model = new CustomerListViewModel
        {
            Search = search,
            Customers = customers
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
            ConfirmPassword = generatedPassword
        };

        return View("Views/Create", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Views/Create", model);
        }

        var result = await _customerService.CreateCustomerAsync(model.FullName, model.Email, model.Company, model.Password, model.Role);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
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
