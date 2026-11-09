namespace ANDtr.Web.Models;

public class RecallNotice
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductCodeName { get; set; } = string.Empty;
    public string RegulationNumber { get; set; } = string.Empty;
    public string WebAddress { get; set; } = string.Empty;
    public string RecallNumber { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public string RecallClass { get; set; } = string.Empty;
    public DateTime ClassificationDate { get; set; }
    public DateTime? PostedDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string? FeiNumber { get; set; }
    public string FirmName { get; set; } = string.Empty;
    public string RecallReason { get; set; } = string.Empty;
    public bool IsDuplicateRecallNumber { get; set; } = false;

    // Navigation property for reviews
    public virtual ICollection<ReviewNotice> ReviewNotices { get; set; } = new List<ReviewNotice>();

    public string RecallClassDisplay => RecallClass switch
    {
        "1" => "Class I",
        "2" => "Class II",
        "3" => "Class III",
        _ => $"Class {RecallClass}"
    };

    public bool IsTerminated => TerminationDate.HasValue;
}
