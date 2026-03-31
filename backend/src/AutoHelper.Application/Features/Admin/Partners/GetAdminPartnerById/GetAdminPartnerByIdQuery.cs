using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Partners.GetAdminPartnerById;

public sealed record GetAdminPartnerByIdQuery(Guid PartnerId)
    : IRequest<Result<AdminPartnerDetailResponse>>;
