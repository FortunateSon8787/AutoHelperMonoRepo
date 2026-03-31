using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Vehicles.GetVehicleByVin;
using AutoHelper.Domain.Vehicles;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Vehicles;

public class GetVehicleByVinQueryHandlerTests
{
    private readonly Mock<IVehicleRepository> _vehicles = new();
    private readonly GetVehicleByVinQueryHandler _sut;

    public GetVehicleByVinQueryHandlerTests()
    {
        _sut = new GetVehicleByVinQueryHandler(_vehicles.Object);
    }

    [Fact]
    public async Task Handle_WhenVehicleExists_ShouldReturnPublicDetails()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var vehicle = Vehicle.Create("1HGCM82633A123456", "Honda", "Accord", 2003, ownerId, color: "Silver", mileage: 80000);

        _vehicles.Setup(r => r.GetByVinAsync("1HGCM82633A123456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        var result = await _sut.Handle(new GetVehicleByVinQuery("1HGCM82633A123456"), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Vin.ShouldBe("1HGCM82633A123456");
        result.Value.Brand.ShouldBe("Honda");
        result.Value.Model.ShouldBe("Accord");
        result.Value.Year.ShouldBe(2003);
        result.Value.Color.ShouldBe("Silver");
        result.Value.Mileage.ShouldBe(80000);
        result.Value.Status.ShouldBe("Active");
    }

    [Fact]
    public async Task Handle_WhenVehicleNotFound_ShouldReturnFailure()
    {
        // Arrange
        _vehicles.Setup(r => r.GetByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        // Act
        var result = await _sut.Handle(new GetVehicleByVinQuery("NOTEXISTENT1234"), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldNotExposeOwnerIdOrDocumentUrl()
    {
        // Arrange
        var vehicle = Vehicle.Create("2T1BURHE0JC123456", "Toyota", "Camry", 2020, Guid.NewGuid());

        _vehicles.Setup(r => r.GetByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        var result = await _sut.Handle(new GetVehicleByVinQuery("2T1BURHE0JC123456"), CancellationToken.None);

        // Assert — PublicVehicleResponse must not have OwnerId or DocumentUrl properties
        result.IsSuccess.ShouldBeTrue();
        var response = result.Value;
        var properties = response.GetType().GetProperties().Select(p => p.Name);
        properties.ShouldNotContain("OwnerId");
        properties.ShouldNotContain("DocumentUrl");
    }

    [Fact]
    public async Task Handle_WhenVehicleIsForSale_ShouldReturnCorrectStatus()
    {
        // Arrange
        var vehicle = Vehicle.Create("1FTFW1ET5DKE12345", "BMW", "X5", 2022, Guid.NewGuid());
        vehicle.ChangeStatus(VehicleStatus.ForSale, partnerName: null, documentUrl: null);

        _vehicles.Setup(r => r.GetByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        var result = await _sut.Handle(new GetVehicleByVinQuery("1FTFW1ET5DKE12345"), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldBe("ForSale");
    }

    [Fact]
    public async Task Handle_WhenVehicleIsInRepair_ShouldIncludePartnerName()
    {
        // Arrange
        var vehicle = Vehicle.Create("3VWFE21C04M000001", "VW", "Golf", 2018, Guid.NewGuid());
        vehicle.ChangeStatus(VehicleStatus.InRepair, partnerName: "AutoService Plus", documentUrl: null);

        _vehicles.Setup(r => r.GetByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        var result = await _sut.Handle(new GetVehicleByVinQuery("3VWFE21C04M000001"), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.PartnerName.ShouldBe("AutoService Plus");
    }
}
