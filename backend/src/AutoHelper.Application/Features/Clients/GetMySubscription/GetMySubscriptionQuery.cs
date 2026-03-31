using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Clients.GetMySubscription;

public sealed record GetMySubscriptionQuery : IRequest<Result<SubscriptionResponse>>;
