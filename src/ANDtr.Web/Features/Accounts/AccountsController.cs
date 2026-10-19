using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ANDtr.Web.Models;

namespace ANDtr.Web.Features.Accounts;

public class AccountsController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountsController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Customer"))
            {
                return RedirectToAction("Index", "SharedReviews");
            }
            return RedirectToAction("Index", "Dashboard");
        }

        var model = new LoginViewModel { ReturnUrl = returnUrl };
        return View("Views/Login", model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        model.ReturnUrl ??= returnUrl;

        if (!ModelState.IsValid)
        {
            return View("Views/Login", model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email address or password.");
            return View("Views/Login", model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "This account has been deactivated. Please contact your administrator.");
            return View("Views/Login", model);
        }

        // Lockout on 5 failed attempts as specified in brief
        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account {Email} locked out due to failed attempts.", model.Email);
            ModelState.AddModelError(string.Empty, "Account locked out after 5 consecutive failed attempts. Please wait 5 minutes and try again.");
            return View("Views/Login", model);
        }

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in successfully.", model.Email);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            if (await _userManager.IsInRoleAsync(user, "Customer"))
            {
                return RedirectToAction("Index", "SharedReviews");
            }

            return RedirectToAction("Index", "Dashboard");
        }

        ModelState.AddModelError(string.Empty, "Invalid email address or password.");
        return View("Views/Login", model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View("Views/AccessDenied");
    }
}
