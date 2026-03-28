using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Partners;
using MediatR;

namespace AutoHelper.Application.Features.Partners.SearchPartnersNearby;

public sealed class SearchPartnersNearbyQueryHandler(
    IPartnerRepository partners)
    : IRequestHandler<SearchPartnersNearbyQuery, Result<IReadOnlyList<PartnerWithDistanceResponse>>>
{
    private const double EarthRadiusKm = 6371.0;
    private const double MaxRadiusKm = 100.0;

    public async Task<Result<IReadOnlyList<PartnerWithDistanceResponse>>> Handle(
        SearchPartnersNearbyQuery request,
        CancellationToken ct)
    {
        var radiusKm = Math.Min(request.RadiusKm, MaxRadiusKm);
        var currentTime = TimeOnly.FromDateTime(DateTime.UtcNow);

        var allPartners = await partners.SearchByLocationAsync(ct);

        PartnerType? typeFilter = null;
        if (request.Type is not null)
        {
            if (!Enum.TryParse<PartnerType>(request.Type, ignoreCase: true, out var parsed))
                return Result<IReadOnlyList<PartnerWithDistanceResponse>>.Success([]);

            typeFilter = parsed;
        }

        var results = allPartners
            .Where(p => typeFilter is null || p.Type == typeFilter)
            .Select(p => (Partner: p, DistanceKm: CalculateHaversineDistance(request.Lat, request.Lng, p.Location.Lat, p.Location.Lng)))
            .Where(x => x.DistanceKm <= radiusKm)
            .Select(x =>
            {
                var isOpenNow = IsPartnerOpenNow(x.Partner.WorkingHours, currentTime);
                return (x.Partner, x.DistanceKm, IsOpenNow: isOpenNow);
            })
            .Where(x => !request.IsOpenNow || x.IsOpenNow)
            .OrderBy(x => x.DistanceKm)
            .Select(x => ToResponse(x.Partner, x.DistanceKm, x.IsOpenNow))
            .ToList();

        return Result<IReadOnlyList<PartnerWithDistanceResponse>>.Success(results);
    }

    private static double CalculateHaversineDistance(double lat1, double lng1, double lat2, double lng2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLng = ToRadians(lng2 - lng1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
              * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

        var c = 2 * Math.Asin(Math.Sqrt(a));

        return EarthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    private static bool IsPartnerOpenNow(WorkingSchedule schedule, TimeOnly currentTime) =>
        currentTime >= schedule.OpenFrom && currentTime <= schedule.OpenTo;

    private static PartnerWithDistanceResponse ToResponse(Partner p, double distanceKm, bool isOpenNow) =>
        new(
            Id: p.Id,
            Name: p.Name,
            Type: p.Type.ToString(),
            Specialization: p.Specialization,
            Description: p.Description,
            Address: p.Address,
            LocationLat: p.Location.Lat,
            LocationLng: p.Location.Lng,
            DistanceKm: Math.Round(distanceKm, 2),
            WorkingOpenFrom: p.WorkingHours.OpenFrom.ToString("HH:mm"),
            WorkingOpenTo: p.WorkingHours.OpenTo.ToString("HH:mm"),
            WorkingDays: p.WorkingHours.WorkDays,
            IsOpenNow: isOpenNow,
            ContactsPhone: p.Contacts.Phone,
            ContactsWebsite: p.Contacts.Website,
            LogoUrl: p.LogoUrl,
            IsVerified: p.IsVerified,
            AverageRating: 0.0,
            ReviewsCount: 0);
}
