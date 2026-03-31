using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Partners.DeletePartner;

public sealed record DeletePartnerCommand(Guid PartnerId) : IRequest<Result>;
