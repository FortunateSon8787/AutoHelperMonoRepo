using AutoHelper.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace AutoHelper.Infrastructure.Ai;

/// <summary>
/// Exposes the three configured model identifiers to the Application layer
/// without leaking Infrastructure types across the boundary.
/// </summary>
public sealed class LlmModelSelector(IOptions<LlmSettings> options) : ILlmModelSelector
{
    public string RouterModel => options.Value.RouterModel;
    public string DefaultModel => options.Value.DefaultModel;
    public string EscalationModel => options.Value.EscalationModel;
}
