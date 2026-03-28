using AutoHelper.Domain.Exceptions;

namespace AutoHelper.Domain.Partners;

/// <summary>
/// Contact information for a partner.
/// </summary>
public sealed record PartnerContacts
{
    /// <summary>Primary contact phone number.</summary>
    public string Phone { get; }

    /// <summary>Optional website URL.</summary>
    public string? Website { get; }

    /// <summary>Optional messenger links (Telegram, WhatsApp, etc.) stored as a plain string or JSON.</summary>
    public string? MessengerLinks { get; }

    private PartnerContacts(string phone, string? website, string? messengerLinks)
    {
        Phone = phone;
        Website = website;
        MessengerLinks = messengerLinks;
    }

    /// <summary>
    /// Creates a validated <see cref="PartnerContacts"/>.
    /// </summary>
    /// <exception cref="DomainException">Thrown when phone is empty.</exception>
    public static PartnerContacts Create(string phone, string? website = null, string? messengerLinks = null)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("Partner phone number must not be empty.");

        return new PartnerContacts(phone.Trim(), website?.Trim(), messengerLinks?.Trim());
    }
}
