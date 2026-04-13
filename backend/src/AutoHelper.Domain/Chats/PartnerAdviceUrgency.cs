namespace AutoHelper.Domain.Chats;

/// <summary>
/// Urgency level for a PartnerAdvice (Mode 3) request as selected by the user.
/// </summary>
public enum PartnerAdviceUrgency
{
    /// <summary>User did not specify urgency.</summary>
    NotSpecified = 0,

    /// <summary>The request is not urgent.</summary>
    NotUrgent = 1,

    /// <summary>The request is urgent.</summary>
    Urgent = 2,
}
