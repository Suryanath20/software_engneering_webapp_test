using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ANDtr.Web.Common;

public class MustChangePasswordFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var mustChange = user.FindFirst("MustChangePassword")?.Value;
            if (string.Equals(mustChange, "true", StringComparison.OrdinalIgnoreCase))
            {
                var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
                var action = context.RouteData.Values["action"]?.ToString() ?? "";

                if (!string.Equals(controller, "Accounts", StringComparison.OrdinalIgnoreCase) ||
                    (!string.Equals(action, "ChangePassword", StringComparison.OrdinalIgnoreCase) &&
                     !string.Equals(action, "Logout", StringComparison.OrdinalIgnoreCase)))
                {
                    context.Result = new RedirectToActionResult("ChangePassword", "Accounts", null);
                }
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
