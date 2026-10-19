using ANDtr.Web.Models;
using ANDtr.Web.Services;

namespace ANDtr.Web.Features.Import;

public class ImportViewModel
{
    public ImportStats Stats { get; set; } = new();
    public List<ImportRecord> RecentImports { get; set; } = new();
    public bool SkipExisting { get; set; } = true;
}
