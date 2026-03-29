namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Provides market price benchmarks for automotive service operations and parts.
/// Used by Mode 2 (WorkClarification) to compare actual service costs against market averages.
/// </summary>
public interface IMarketPriceGateway
{
    /// <summary>
    /// Returns a formatted string describing market price ranges for the given works.
    /// Returns null when benchmarks cannot be determined (e.g., unknown operations).
    /// The returned string is injected into the LLM system prompt as MARKET_BENCHMARKS.
    /// </summary>
    Task<string?> GetMarketPriceBenchmarksAsync(
        string worksDescription,
        decimal actualLaborCost,
        decimal actualPartsCost,
        CancellationToken ct);
}
