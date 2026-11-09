namespace ANDtr.Web.Models;

/// <summary>
/// Audit log entry tracking dataset import runs and stats.
/// </summary>
public class ImportRecord
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

    public string? ImportedById { get; set; }
    public virtual ApplicationUser? ImportedBy { get; set; }

    public int RowsRead { get; set; }

    public int RowsImported { get; set; }

    public int RowsSkipped { get; set; }

    public int DuplicatesFlagged { get; set; }

    public string Status { get; set; } = "Complete";

    public string? Details { get; set; }
}
