using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Import;

[Authorize(Roles = "Employee")]
public class ImportController : Controller
{
    private readonly IImportService _importService;

    public ImportController(IImportService importService)
    {
        _importService = importService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var stats = await _importService.GetImportStatsAsync();
        var history = await _importService.GetRecentImportsAsync(10);

        var model = new ImportViewModel
        {
            Stats = stats,
            RecentImports = history,
            SkipExisting = true
        };

        return View("Import", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile? file, bool skipExisting = true)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ImportResult result;

        if (file != null && file.Length > 0)
        {
            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Only Excel spreadsheet files (.xlsx) are supported.";
                return RedirectToAction(nameof(Index));
            }

            await using var stream = file.OpenReadStream();
            result = await _importService.ImportFromStreamAsync(stream, file.FileName, userId, skipExisting);
        }
        else
        {
            // Default file fallback
            result = await _importService.ImportFromDefaultFileAsync(userId, skipExisting);
        }

        if (result.Success)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
