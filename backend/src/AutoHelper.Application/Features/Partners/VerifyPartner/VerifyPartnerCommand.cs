using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Partners.VerifyPartner;

/// <summary>
/// Verifies a partner profile (admin-only operation).
/// Sets IsVerified = true and IsActive = true.
/// </summary>
public sealed record VerifyPartnerCommand(Guid PartnerId) : IRequest<Result>;
