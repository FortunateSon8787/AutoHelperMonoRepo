using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Partners.GetAdminPartners;

public sealed record GetAdminPartnersQuery(int Page, int PageSize, string? Search)
    : IRequest<Result<AdminPartnerListResponse>>;
