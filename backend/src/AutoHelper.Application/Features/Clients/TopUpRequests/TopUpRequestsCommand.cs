using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Clients.TopUpRequests;

/// <summary>
/// One-time top-up: adds <see cref="Count"/> AI requests to the customer's remaining quota.
/// </summary>
public sealed record TopUpRequestsCommand(int Count) : IRequest<Result>;
