namespace AutoHelper.Domain.AdCampaigns;

/// <summary>
/// Tracks impression and click statistics for an ad campaign.
/// Owned value object — stored as columns on the ad_campaigns table.
/// </summary>
public sealed class AdStats
{
    public int Impressions { get; private set; }
    public int Clicks { get; private set; }

    // EF Core
    private AdStats() { }

    public static AdStats Zero() => new() { Impressions = 0, Clicks = 0 };

    public void RecordImpression() => Impressions++;
    public void RecordClick() => Clicks++;
}
