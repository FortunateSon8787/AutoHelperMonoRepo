namespace AutoHelper.Domain.Chats;

/// <summary>
/// Input provided by the user when creating a PartnerAdvice (Mode 3) chat session.
/// Contains the user's request description and geolocation for proximity search.
/// </summary>
public sealed class PartnerAdviceInput
{
    /// <summary>What the user needs (e.g. "my car won't start, need a tow truck").</summary>
    public string Request { get; init; } = string.Empty;

    /// <summary>User latitude for proximity partner search.</summary>
    public double Lat { get; init; }

    /// <summary>User longitude for proximity partner search.</summary>
    public double Lng { get; init; }

    /// <summary>Optional urgency hint provided by the user.</summary>
    public string? Urgency { get; init; }
}
