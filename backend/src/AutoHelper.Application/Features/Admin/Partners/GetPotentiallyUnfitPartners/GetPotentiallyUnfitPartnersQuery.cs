using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Partners.GetPotentiallyUnfitPartners;

public sealed record GetPotentiallyUnfitPartnersQuery : IRequest<Result<IReadOnlyList<AdminUnfitPartnerResponse>>>;
