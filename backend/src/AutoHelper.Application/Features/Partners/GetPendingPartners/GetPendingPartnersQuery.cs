using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Partners.GetPendingPartners;

/// <summary>Returns all partner profiles awaiting administrator verification.</summary>
public sealed record GetPendingPartnersQuery : IRequest<Result<IReadOnlyList<PartnerResponse>>>;
