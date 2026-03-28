using AutoHelper.Domain.Exceptions;
using AutoHelper.Domain.Partners;
using Shouldly;

namespace AutoHelper.Domain.Tests.Partners;

public class GeoPointTests
{
    [Fact]
    public void Create_WithValidCoordinates_ShouldReturnGeoPoint()
    {
        var point = GeoPoint.Create(55.75, 37.61);

        point.Lat.ShouldBe(55.75);
        point.Lng.ShouldBe(37.61);
    }

    [Theory]
    [InlineData(-90)]
    [InlineData(0)]
    [InlineData(90)]
    public void Create_WithBoundaryLatitude_ShouldNotThrow(double lat)
    {
        Should.NotThrow(() => GeoPoint.Create(lat, 0));
    }

    [Theory]
    [InlineData(-180)]
    [InlineData(0)]
    [InlineData(180)]
    public void Create_WithBoundaryLongitude_ShouldNotThrow(double lng)
    {
        Should.NotThrow(() => GeoPoint.Create(0, lng));
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Create_WithInvalidLatitude_ShouldThrowDomainException(double lat)
    {
        Should.Throw<DomainException>(() => GeoPoint.Create(lat, 0))
            .Message.ShouldContain("Latitude");
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void Create_WithInvalidLongitude_ShouldThrowDomainException(double lng)
    {
        Should.Throw<DomainException>(() => GeoPoint.Create(0, lng))
            .Message.ShouldContain("Longitude");
    }

    [Fact]
    public void TwoGeoPoints_WithSameCoordinates_ShouldBeEqual()
    {
        var a = GeoPoint.Create(55.75, 37.61);
        var b = GeoPoint.Create(55.75, 37.61);

        a.ShouldBe(b);
    }
}
