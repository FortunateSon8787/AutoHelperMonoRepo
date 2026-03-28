using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Partners.GetPartnerById;

/// <summary>Returns the public profile of a verified and active partner by their unique identifier.</summary>
public sealed record GetPartnerByIdQuery(Guid PartnerId) : IRequest<Result<PartnerResponse>>;
