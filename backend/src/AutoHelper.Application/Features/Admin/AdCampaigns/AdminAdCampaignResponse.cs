using AutoHelper.Domain.AdCampaigns;

namespace AutoHelper.Application.Features.Admin.AdCampaigns;

public sealed record AdminAdCampaignResponse(
    Guid Id,
    Guid PartnerId,
    string Type,
    string TargetCategory,
    string Content,
    DateTime StartsAt,
    DateTime EndsAt,
    bool IsActive,
    bool ShowToAnonymous,
    int StatsImpressions,
    int StatsClicks)
{
    public static AdminAdCampaignResponse FromCampaign(AdCampaign c) => new(
        Id: c.Id,
        PartnerId: c.PartnerId,
        Type: c.Type.ToString(),
        TargetCategory: c.TargetCategory.ToString(),
        Content: c.Content,
        StartsAt: c.StartsAt,
        EndsAt: c.EndsAt,
        IsActive: c.IsActive,
        ShowToAnonymous: c.ShowToAnonymous,
        StatsImpressions: c.Stats.Impressions,
        StatsClicks: c.Stats.Clicks);
}

public sealed record AdminAdCampaignListResponse(
    IReadOnlyList<AdminAdCampaignResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
