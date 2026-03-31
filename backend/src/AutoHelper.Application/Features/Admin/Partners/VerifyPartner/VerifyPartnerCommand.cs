using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Partners.VerifyPartner;

public sealed record VerifyPartnerCommand(Guid PartnerId) : IRequest<Result>;
