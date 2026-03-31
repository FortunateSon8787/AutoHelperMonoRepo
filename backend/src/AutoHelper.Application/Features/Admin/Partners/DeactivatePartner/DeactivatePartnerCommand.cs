using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Partners.DeactivatePartner;

public sealed record DeactivatePartnerCommand(Guid PartnerId) : IRequest<Result>;
