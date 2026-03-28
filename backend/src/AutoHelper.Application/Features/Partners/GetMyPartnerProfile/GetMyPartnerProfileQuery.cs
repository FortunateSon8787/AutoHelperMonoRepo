using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Partners.GetMyPartnerProfile;

/// <summary>Returns the partner profile of the currently authenticated partner user.</summary>
public sealed record GetMyPartnerProfileQuery : IRequest<Result<PartnerResponse>>;
