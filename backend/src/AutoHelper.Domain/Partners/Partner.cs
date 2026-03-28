using AutoHelper.Domain.Common;
using AutoHelper.Domain.Exceptions;
using AutoHelper.Domain.Partners.Events;

namespace AutoHelper.Domain.Partners;

/// <summary>
/// Aggregate root representing a registered partner (auto service, car wash, towing, etc.).
/// A partner account is distinct from a customer account and has its own partner cabinet.
/// A new partner is inactive until verified by an administrator.
/// </summary>
public sealed class Partner : AggregateRoot<Guid>
{
    // ─── Core fields ──────────────────────────────────────────────────────────

    public string Name { get; private set; } = string.Empty;
    public PartnerType Type { get; private set; }
    public string Specialization { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;

    // ─── Value Objects ────────────────────────────────────────────────────────

    public GeoPoint Location { get; private set; } = null!;
    public WorkingSchedule WorkingHours { get; private set; } = null!;
    public PartnerContacts Contacts { get; private set; } = null!;

    // ─── Optional fields ──────────────────────────────────────────────────────

    /// <summary>URL of the partner's logo stored in object storage.</summary>
    public string? LogoUrl { get; private set; }

    // ─── Status flags ─────────────────────────────────────────────────────────

    /// <summary>Set to true by an administrator after manual verification.</summary>
    public bool IsVerified { get; private set; }

    /// <summary>Active partners are visible in the platform. Requires verification first.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Automatically set to true when a partner accumulates 5 or more ratings below 3.</summary>
    public bool IsPotentiallyUnfit { get; private set; }

    /// <summary>Controls whether advertising banners are shown to anonymous (unauthenticated) users.</summary>
    public bool ShowBannersToAnonymous { get; private set; }

    // ─── Ownership ────────────────────────────────────────────────────────────

    /// <summary>The Customer account that owns this partner profile.</summary>
    public Guid AccountUserId { get; private set; }

    // ─── Soft-delete ──────────────────────────────────────────────────────────

    public bool IsDeleted { get; private set; }

    // ─── EF Core ──────────────────────────────────────────────────────────────

    private Partner() { }

    // ─── Factory method ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new partner profile awaiting administrator verification.
    /// The partner is inactive by default until <see cref="Verify"/> is called.
    /// </summary>
    /// <exception cref="DomainException">Thrown when required string fields are empty.</exception>
    public static Partner Create(
        string name,
        PartnerType type,
        string specialization,
        string description,
        string address,
        GeoPoint location,
        WorkingSchedule workingHours,
        PartnerContacts contacts,
        Guid accountUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Partner name must not be empty.");

        if (string.IsNullOrWhiteSpace(specialization))
            throw new DomainException("Partner specialization must not be empty.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Partner description must not be empty.");

        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("Partner address must not be empty.");

        if (accountUserId == Guid.Empty)
            throw new DomainException("Partner must be linked to a valid user account.");

        var partner = new Partner
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Type = type,
            Specialization = specialization.Trim(),
            Description = description.Trim(),
            Address = address.Trim(),
            Location = location,
            WorkingHours = workingHours,
            Contacts = contacts,
            AccountUserId = accountUserId,
            IsVerified = false,
            IsActive = false,
            IsPotentiallyUnfit = false,
            ShowBannersToAnonymous = false,
            IsDeleted = false
        };

        partner.AddDomainEvent(new PartnerRegisteredEvent(partner.Id, accountUserId));
        return partner;
    }

    // ─── Business operations ──────────────────────────────────────────────────

    /// <summary>
    /// Updates the partner's public profile information.
    /// </summary>
    /// <exception cref="DomainException">Thrown when required string fields are empty.</exception>
    public void UpdateProfile(
        string name,
        string specialization,
        string description,
        string address,
        GeoPoint location,
        WorkingSchedule workingHours,
        PartnerContacts contacts)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Partner name must not be empty.");

        if (string.IsNullOrWhiteSpace(specialization))
            throw new DomainException("Partner specialization must not be empty.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Partner description must not be empty.");

        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("Partner address must not be empty.");

        Name = name.Trim();
        Specialization = specialization.Trim();
        Description = description.Trim();
        Address = address.Trim();
        Location = location;
        WorkingHours = workingHours;
        Contacts = contacts;
    }

    /// <summary>Sets or replaces the partner's logo URL.</summary>
    public void UpdateLogo(string logoUrl)
    {
        if (string.IsNullOrWhiteSpace(logoUrl))
            throw new DomainException("Logo URL must not be empty.");

        LogoUrl = logoUrl.Trim();
    }

    /// <summary>
    /// Marks the partner as verified and activates their profile.
    /// Only an administrator should call this operation.
    /// </summary>
    public void Verify()
    {
        IsVerified = true;
        IsActive = true;
    }

    /// <summary>Deactivates the partner's profile (e.g. suspension by administrator).</summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>Controls whether advertising banners are shown to anonymous users.</summary>
    public void SetBannerVisibility(bool showToAnonymous)
    {
        ShowBannersToAnonymous = showToAnonymous;
    }

    /// <summary>
    /// Soft-deletes the partner profile.
    /// Physical deletion is not permitted per platform policy.
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        IsActive = false;
    }
}
