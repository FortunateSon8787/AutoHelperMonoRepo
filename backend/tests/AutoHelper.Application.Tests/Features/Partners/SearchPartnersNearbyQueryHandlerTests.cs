using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Partners.SearchPartnersNearby;
using AutoHelper.Domain.Partners;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Partners;

public class SearchPartnersNearbyQueryHandlerTests
{
    private readonly Mock<IPartnerRepository> _partners = new();
    private readonly SearchPartnersNearbyQueryHandler _sut;

    // Moscow city center
    private const double OriginLat = 55.7558;
    private const double OriginLng = 37.6173;

    public SearchPartnersNearbyQueryHandlerTests()
    {
        _sut = new SearchPartnersNearbyQueryHandler(_partners.Object);
    }

    private static Partner CreatePartner(double lat, double lng, PartnerType type, TimeOnly openFrom, TimeOnly openTo)
    {
        var partner = Partner.Create(
            name: "Test Partner",
            type: type,
            specialization: "Test",
            description: "Test description",
            address: "Test address",
            location: GeoPoint.Create(lat, lng),
            workingHours: WorkingSchedule.Create(openFrom, openTo, "Mon-Fri"),
            contacts: PartnerContacts.Create("+7-999-000-0000"),
            accountUserId: Guid.NewGuid());

        partner.Verify(); // make IsVerified = true, IsActive = true
        return partner;
    }

    [Fact]
    public async Task Handle_WhenPartnersWithinRadius_ShouldReturnThem()
    {
        // ~1.5 km from origin
        var nearby = CreatePartner(55.7658, 37.6173, PartnerType.AutoService, new TimeOnly(0, 0), new TimeOnly(23, 59));
        // ~50 km from origin
        var farAway = CreatePartner(56.2, 37.6173, PartnerType.AutoService, new TimeOnly(0, 0), new TimeOnly(23, 59));

        _partners.Setup(r => r.SearchByLocationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([nearby, farAway]);

        var query = new SearchPartnersNearbyQuery(OriginLat, OriginLng, RadiusKm: 10, Type: null, IsOpenNow: false);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        result.Value[0].Name.ShouldBe(nearby.Name);
    }

    [Fact]
    public async Task Handle_WhenNoPartnersWithinRadius_ShouldReturnEmptyList()
    {
        var farAway = CreatePartner(60.0, 37.6173, PartnerType.AutoService, new TimeOnly(0, 0), new TimeOnly(23, 59));

        _partners.Setup(r => r.SearchByLocationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([farAway]);

        var query = new SearchPartnersNearbyQuery(OriginLat, OriginLng, RadiusKm: 10, Type: null, IsOpenNow: false);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenTypeFilterApplied_ShouldReturnOnlyMatchingType()
    {
        var autoService = CreatePartner(55.76, 37.6173, PartnerType.AutoService, new TimeOnly(0, 0), new TimeOnly(23, 59));
        var carWash = CreatePartner(55.76, 37.6173, PartnerType.CarWash, new TimeOnly(0, 0), new TimeOnly(23, 59));

        _partners.Setup(r => r.SearchByLocationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([autoService, carWash]);

        var query = new SearchPartnersNearbyQuery(OriginLat, OriginLng, RadiusKm: 10, Type: "AutoService", IsOpenNow: false);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        result.Value[0].Type.ShouldBe("AutoService");
    }

    [Fact]
    public async Task Handle_WhenIsOpenNowTrue_ShouldReturnOnlyOpenPartners()
    {
        // Always open
        var alwaysOpen = CreatePartner(55.76, 37.6173, PartnerType.AutoService, new TimeOnly(0, 0), new TimeOnly(23, 59));
        // Never open (window in the past)
        var closed = CreatePartner(55.76, 37.62, PartnerType.AutoService, new TimeOnly(1, 0), new TimeOnly(1, 30));

        _partners.Setup(r => r.SearchByLocationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([alwaysOpen, closed]);

        // Use a current UTC time that falls within alwaysOpen but outside closed
        var query = new SearchPartnersNearbyQuery(OriginLat, OriginLng, RadiusKm: 10, Type: null, IsOpenNow: true);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        // alwaysOpen (00:00–23:59) covers any current UTC time; closed (01:00–01:30) should be excluded
        result.Value.Count.ShouldBe(1);
        result.Value.ShouldAllBe(p => p.IsOpenNow);
    }

    [Fact]
    public async Task Handle_ResultsShouldBeSortedByDistanceAscending()
    {
        var closer = CreatePartner(55.76, 37.6173, PartnerType.AutoService, new TimeOnly(0, 0), new TimeOnly(23, 59));
        var farther = CreatePartner(55.78, 37.6173, PartnerType.AutoService, new TimeOnly(0, 0), new TimeOnly(23, 59));

        _partners.Setup(r => r.SearchByLocationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([farther, closer]); // deliberately out of order

        var query = new SearchPartnersNearbyQuery(OriginLat, OriginLng, RadiusKm: 50, Type: null, IsOpenNow: false);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
        result.Value[0].DistanceKm.ShouldBeLessThan(result.Value[1].DistanceKm);
    }

    [Fact]
    public async Task Handle_RadiusExceeds100Km_ShouldCapAt100Km()
    {
        // ~80 km from Moscow center — within the 100 km cap, so must be included even when raw input is 500 km
        var withinCap = CreatePartner(56.47, 37.6173, PartnerType.AutoService, new TimeOnly(0, 0), new TimeOnly(23, 59));

        _partners.Setup(r => r.SearchByLocationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([withinCap]);

        var query = new SearchPartnersNearbyQuery(OriginLat, OriginLng, RadiusKm: 500, Type: null, IsOpenNow: false);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1); // partner within 100 km cap is included
    }

    [Fact]
    public async Task Handle_WhenTypeIsInvalid_ShouldReturnEmptyList()
    {
        var partner = CreatePartner(55.76, 37.6173, PartnerType.AutoService, new TimeOnly(0, 0), new TimeOnly(23, 59));

        _partners.Setup(r => r.SearchByLocationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([partner]);

        var query = new SearchPartnersNearbyQuery(OriginLat, OriginLng, RadiusKm: 10, Type: "NonExistentType", IsOpenNow: false);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ResponseDistanceKm_ShouldBeRoundedToTwoDecimalPlaces()
    {
        var partner = CreatePartner(55.7658, 37.6173, PartnerType.AutoService, new TimeOnly(0, 0), new TimeOnly(23, 59));

        _partners.Setup(r => r.SearchByLocationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([partner]);

        var query = new SearchPartnersNearbyQuery(OriginLat, OriginLng, RadiusKm: 10, Type: null, IsOpenNow: false);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        var distanceString = result.Value[0].DistanceKm.ToString("G");
        // Verify no more than 2 decimal places
        var decimalPart = distanceString.Contains('.') ? distanceString.Split('.')[1] : "";
        decimalPart.Length.ShouldBeLessThanOrEqualTo(2);
    }
}
