using Microsoft.AspNetCore.Mvc;

namespace ANDtr.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
