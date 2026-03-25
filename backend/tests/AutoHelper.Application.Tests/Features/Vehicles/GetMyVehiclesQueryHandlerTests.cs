using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Vehicles;
using AutoHelper.Application.Features.Vehicles.GetMyVehicles;
using AutoHelper.Domain.Vehicles;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Vehicles;

public class GetMyVehiclesQueryHandlerTests
{
    private readonly Mock<IVehicleRepository> _vehicles = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly GetMyVehiclesQueryHandler _sut;

    private static readonly Guid UserId = Guid.NewGuid();

    public GetMyVehiclesQueryHandlerTests()
    {
        _sut = new GetMyVehiclesQueryHandler(_vehicles.Object, _currentUser.Object);
        _currentUser.Setup(u => u.Id).Returns(UserId);
    }

    [Fact]
    public async Task Handle_WhenUserHasVehicles_ShouldReturnAllVehicles()
    {
        // Arrange
        var v1 = Vehicle.Create("1HGCM82633A123456", "Honda", "Accord", 2003, UserId);
        var v2 = Vehicle.Create("2T1BURHE0JC123456", "Toyota", "Camry", 2020, UserId);

        _vehicles.Setup(r => r.GetAllByOwnerIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([v1, v2]);

        // Act
        var result = await _sut.Handle(new GetMyVehiclesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
        result.Value.ShouldContain(v => v.Vin == "1HGCM82633A123456");
        result.Value.ShouldContain(v => v.Vin == "2T1BURHE0JC123456");
    }

    [Fact]
    public async Task Handle_WhenUserHasNoVehicles_ShouldReturnEmptyList()
    {
        // Arrange
        _vehicles.Setup(r => r.GetAllByOwnerIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(new GetMyVehiclesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUser.Setup(u => u.Id).Returns((Guid?)null);

        // Act
        var result = await _sut.Handle(new GetMyVehiclesQuery(), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _vehicles.Verify(r => r.GetAllByOwnerIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldMapVehicleFieldsCorrectly()
    {
        // Arrange
        var vehicle = Vehicle.Create("1FTFW1ET5DKE12345", "Ford", "Focus", 2018, UserId, "Red", 25000);
        _vehicles.Setup(r => r.GetAllByOwnerIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([vehicle]);

        // Act
        var result = await _sut.Handle(new GetMyVehiclesQuery(), CancellationToken.None);

        // Assert
        var response = result.Value.Single();
        response.Vin.ShouldBe("1FTFW1ET5DKE12345");
        response.Brand.ShouldBe("Ford");
        response.Model.ShouldBe("Focus");
        response.Year.ShouldBe(2018);
        response.Color.ShouldBe("Red");
        response.Mileage.ShouldBe(25000);
        response.Status.ShouldBe(VehicleResponse.FromVehicle(vehicle).Status);
    }
}
