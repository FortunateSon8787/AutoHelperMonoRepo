using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Clients.ActivateSubscription;

/// <summary>
/// Activates or upgrades a subscription plan for the current customer.
/// Billing via the external IBillingService (Lemon Squeezy integration will be added separately).
/// </summary>
public sealed record ActivateSubscriptionCommand(string Plan) : IRequest<Result>;
