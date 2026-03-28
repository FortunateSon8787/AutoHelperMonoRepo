using AutoHelper.Domain.Common;
using AutoHelper.Domain.Exceptions;
using AutoHelper.Domain.Partners;

namespace AutoHelper.Domain.AdCampaigns;

/// <summary>
/// Represents an advertising campaign created by a partner.
/// A campaign can be an OfferBlock or Banner, targeting a specific service category.
/// </summary>
public sealed class AdCampaign : AggregateRoot<Guid>
{
    // ─── Core fields ──────────────────────────────────────────────────────────

    public Guid PartnerId { get; private set; }
    public AdType Type { get; private set; }

    /// <summary>Targets ads to users interested in this service category.</summary>
    public PartnerType TargetCategory { get; private set; }

    /// <summary>Ad content: text for OfferBlock or image URL for Banner.</summary>
    public string Content { get; private set; } = string.Empty;

    // ─── Schedule ─────────────────────────────────────────────────────────────

    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }

    // ─── Status ───────────────────────────────────────────────────────────────

    public bool IsActive { get; private set; }

    /// <summary>Whether this campaign's banners are visible to anonymous (unauthenticated) users.</summary>
    public bool ShowToAnonymous { get; private set; }

    // ─── Stats ────────────────────────────────────────────────────────────────

    public AdStats Stats { get; private set; } = null!;

    // ─── Soft-delete ──────────────────────────────────────────────────────────

    public bool IsDeleted { get; private set; }

    // ─── EF Core ──────────────────────────────────────────────────────────────

    private AdCampaign() { }

    // ─── Factory method ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new ad campaign for a partner.
    /// The campaign is inactive by default until explicitly activated.
    /// </summary>
    /// <exception cref="DomainException">Thrown when required fields are invalid.</exception>
    public static AdCampaign Create(
        Guid partnerId,
        AdType type,
        PartnerType targetCategory,
        string content,
        DateTime startsAt,
        DateTime endsAt,
        bool showToAnonymous)
    {
        if (partnerId == Guid.Empty)
            throw new DomainException("Ad campaign must be linked to a valid partner.");

        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Ad campaign content must not be empty.");

        if (endsAt <= startsAt)
            throw new DomainException("Ad campaign end date must be after start date.");

        return new AdCampaign
        {
            Id = Guid.NewGuid(),
            PartnerId = partnerId,
            Type = type,
            TargetCategory = targetCategory,
            Content = content.Trim(),
            StartsAt = startsAt.ToUniversalTime(),
            EndsAt = endsAt.ToUniversalTime(),
            IsActive = false,
            ShowToAnonymous = showToAnonymous,
            Stats = AdStats.Zero(),
            IsDeleted = false
        };
    }

    // ─── Business operations ──────────────────────────────────────────────────

    /// <summary>Updates the campaign schedule and content.</summary>
    /// <exception cref="DomainException">Thrown when required fields are invalid.</exception>
    public void Update(
        AdType type,
        PartnerType targetCategory,
        string content,
        DateTime startsAt,
        DateTime endsAt,
        bool showToAnonymous)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Ad campaign content must not be empty.");

        if (endsAt <= startsAt)
            throw new DomainException("Ad campaign end date must be after start date.");

        Type = type;
        TargetCategory = targetCategory;
        Content = content.Trim();
        StartsAt = startsAt.ToUniversalTime();
        EndsAt = endsAt.ToUniversalTime();
        ShowToAnonymous = showToAnonymous;
    }

    /// <summary>Activates the campaign so it becomes eligible for display.</summary>
    public void Activate() => IsActive = true;

    /// <summary>Deactivates the campaign — it will no longer be shown.</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Determines whether this campaign should be displayed to the given visitor.
    /// Partners never see ad banners from other partners (rule 13).
    /// Anonymous users only see the campaign if <see cref="ShowToAnonymous"/> is true.
    /// </summary>
    public bool IsVisibleTo(bool isAuthenticated, bool isPartner)
    {
        if (!IsActive || IsDeleted)
            return false;

        if (isPartner)
            return false;

        if (!isAuthenticated && !ShowToAnonymous)
            return false;

        return true;
    }

    /// <summary>Soft-deletes the campaign.</summary>
    public void Delete()
    {
        IsDeleted = true;
        IsActive = false;
    }
}
