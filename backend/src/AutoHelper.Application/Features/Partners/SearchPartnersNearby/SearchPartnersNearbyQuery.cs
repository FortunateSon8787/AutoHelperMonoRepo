using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Partners.SearchPartnersNearby;

/// <summary>
/// Returns verified and active partners within the specified radius, optionally filtered by type and open status.
/// </summary>
public sealed record SearchPartnersNearbyQuery(
    double Lat,
    double Lng,
    double RadiusKm,
    string? Type,
    bool IsOpenNow) : IRequest<Result<IReadOnlyList<PartnerWithDistanceResponse>>>;
