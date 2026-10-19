namespace ANDtr.Web.Models;

/// <summary>
/// Represents an FDA medical device safety and recall notice.
/// </summary>
public class RecallNotice
{
    public int Id { get; set; }

    /// <summary>3-letter FDA Product Code (e.g. BSZ, CAC, JOH)</summary>
    public string ProductCode { get; set; } = string.Empty;

    public string ProductCodeName { get; set; } = string.Empty;

    public string RegulationNumber { get; set; } = string.Empty;

    public string WebAddress { get; set; } = string.Empty;

    /// <summary>FDA Recall Number (e.g. Z-1706-2015)</summary>
    public string RecallNumber { get; set; } = string.Empty;

    public string ProductDescription { get; set; } = string.Empty;

    public string? TradeName { get; set; }

    /// <summary>Classification: "1" (Class I), "2" (Class II), "3" (Class III)</summary>
    public string RecallClass { get; set; } = string.Empty;

    public DateTime ClassificationDate { get; set; }

    public DateTime? PostedDate { get; set; }

    public DateTime? TerminationDate { get; set; }

    public string? FeiNumber { get; set; }

    public string FirmName { get; set; } = string.Empty;

    public string RecallReason { get; set; } = string.Empty;

    /// <summary>Flag indicating if this recall number appears on multiple distinct product codes.</summary>
    public bool IsDuplicateRecallNumber { get; set; } = false;

    // Navigation properties
    public virtual ICollection<ReviewNotice> ReviewNotices { get; set; } = new List<ReviewNotice>();

    /// <summary>Helper property returning human-friendly class text ("Class I", "Class II", "Class III")</summary>
    public string RecallClassDisplay => RecallClass switch
    {
        "1" => "Class I",
        "2" => "Class II",
        "3" => "Class III",
        _ => $"Class {RecallClass}"
    };

    /// <summary>Helper property returning true if recall is terminated</summary>
    public bool IsTerminated => TerminationDate.HasValue;
}
