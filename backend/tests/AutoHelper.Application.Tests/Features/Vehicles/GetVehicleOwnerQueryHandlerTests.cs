using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Vehicles.GetVehicleOwner;
using AutoHelper.Domain.Customers;
using AutoHelper.Domain.Vehicles;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Vehicles;

public class GetVehicleOwnerQueryHandlerTests
{
    private readonly Mock<IVehicleRepository> _vehicles = new();
    private readonly Mock<ICustomerRepository> _customers = new();
    private readonly GetVehicleOwnerQueryHandler _sut;

    public GetVehicleOwnerQueryHandlerTests()
    {
        _sut = new GetVehicleOwnerQueryHandler(_vehicles.Object, _customers.Object);
    }

    [Fact]
    public async Task Handle_WhenVehicleExists_ShouldReturnOwnerProfile()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var vehicle = Vehicle.Create("1HGCM82633A123456", "Honda", "Accord", 2003, ownerId);
        var owner = Customer.CreateWithPassword("Ivan Ivanov", "ivan@test.com", "hash", contacts: "+7 999 000-00-00");

        _vehicles.Setup(r => r.GetByVinAsync("1HGCM82633A123456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);
        _customers.Setup(r => r.GetByIdAsync(vehicle.OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner);

        // Act
        var result = await _sut.Handle(new GetVehicleOwnerQuery("1HGCM82633A123456"), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Ivan Ivanov");
        result.Value.Contacts.ShouldBe("+7 999 000-00-00");
    }

    [Fact]
    public async Task Handle_WhenVehicleNotFound_ShouldReturnFailure()
    {
        // Arrange
        _vehicles.Setup(r => r.GetByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        // Act
        var result = await _sut.Handle(new GetVehicleOwnerQuery("NONEXISTENT"), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeEmpty();
        _customers.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOwnerNotFound_ShouldReturnFailure()
    {
        // Arrange
        var vehicle = Vehicle.Create("VIN123456789", "Toyota", "Camry", 2020, Guid.NewGuid());

        _vehicles.Setup(r => r.GetByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);
        _customers.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _sut.Handle(new GetVehicleOwnerQuery("VIN123456789"), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenOwnerHasNoContacts_ShouldReturnNullContacts()
    {
        // Arrange
        var vehicle = Vehicle.Create("VIN999999999", "BMW", "X5", 2022, Guid.NewGuid());
        var owner = Customer.CreateWithPassword("No Contacts", "nocontacts@test.com", "hash");

        _vehicles.Setup(r => r.GetByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);
        _customers.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner);

        // Act
        var result = await _sut.Handle(new GetVehicleOwnerQuery("VIN999999999"), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Contacts.ShouldBeNull();
    }
}
