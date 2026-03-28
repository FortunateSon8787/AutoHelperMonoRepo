using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Partners.DeactivatePartner;

/// <summary>Deactivates a partner profile (admin-only operation).</summary>
public sealed record DeactivatePartnerCommand(Guid PartnerId) : IRequest<Result>;
