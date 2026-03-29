namespace AutoHelper.Domain.Chats;

/// <summary>
/// Initial form submitted by the customer when starting a WorkClarification (Mode 2) chat.
/// Describes the work performed, costs, and guarantees received at the service center.
/// </summary>
public sealed record WorkClarificationInput
{
    /// <summary>List of operations performed and parts used (free text).</summary>
    public string WorksPerformed { get; init; } = string.Empty;

    /// <summary>Reason the service center gave for performing the work.</summary>
    public string WorkReason { get; init; } = string.Empty;

    /// <summary>Total cost of labor as reported by the service center.</summary>
    public decimal LaborCost { get; init; }

    /// <summary>Total cost of parts as reported by the service center.</summary>
    public decimal PartsCost { get; init; }

    /// <summary>Guarantees and promises given by the service center (free text).</summary>
    public string? Guarantees { get; init; }
}
