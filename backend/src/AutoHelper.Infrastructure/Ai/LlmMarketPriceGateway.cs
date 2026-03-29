using AutoHelper.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutoHelper.Infrastructure.Ai;

/// <summary>
/// IMarketPriceGateway implementation that uses the LLM (router model) to estimate
/// market price ranges for automotive service operations.
/// This is a best-effort estimation; the result is always injected with a disclaimer
/// that figures are approximate and region-dependent.
/// </summary>
public sealed class LlmMarketPriceGateway(
    ILlmProvider llm,
    ILlmModelSelector modelSelector,
    ILogger<LlmMarketPriceGateway> logger) : IMarketPriceGateway
{
    private const string SystemPrompt =
        "You are an automotive service pricing analyst. " +
        "Given a description of work performed and the actual costs charged, " +
        "estimate the typical market price range for this type of work in Russia (Moscow region as baseline). " +
        "Output a concise plain-text summary of market benchmarks — one paragraph, no JSON. " +
        "Include: typical labor cost range, typical parts cost range, and whether the total cost is reasonable. " +
        "Always note that prices are approximate and vary by region and vehicle class. " +
        "If the work description is too vague to estimate, respond with 'Market benchmarks unavailable for the described work.'";

    public async Task<string?> GetMarketPriceBenchmarksAsync(
        string worksDescription,
        decimal actualLaborCost,
        decimal actualPartsCost,
        CancellationToken ct)
    {
        var userInput =
            $"Works: {worksDescription}\n" +
            $"Actual labor cost: {actualLaborCost:F0} RUB\n" +
            $"Actual parts cost: {actualPartsCost:F0} RUB";

        try
        {
            var result = await llm.GenerateTextAsync(
                modelSelector.RouterModel, SystemPrompt, userInput, ct);

            if (result.Contains("unavailable", StringComparison.OrdinalIgnoreCase))
                return null;

            return result;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "LlmMarketPriceGateway failed to fetch benchmarks");
            return null;
        }
    }
}
