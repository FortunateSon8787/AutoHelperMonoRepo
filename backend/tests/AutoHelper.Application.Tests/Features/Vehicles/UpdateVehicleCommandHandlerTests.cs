using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Vehicles.UpdateVehicle;
using AutoHelper.Domain.Vehicles;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Vehicles;

public class UpdateVehicleCommandHandlerTests
{
    private readonly Mock<IVehicleRepository> _vehicles = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateVehicleCommandHandler _sut;

    private static readonly Guid UserId = Guid.NewGuid();

    public UpdateVehicleCommandHandlerTests()
    {
        _sut = new UpdateVehicleCommandHandler(
            _vehicles.Object,
            _currentUser.Object,
            _unitOfWork.Object);

        _currentUser.Setup(u => u.Id).Returns(UserId);
    }

    [Fact]
    public async Task Handle_WhenVehicleFoundAndOwned_ShouldUpdateAndSave()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var vehicle = Vehicle.Create("1HGCM82633A123456", "Honda", "Accord", 2003, UserId);
        _vehicles.Setup(r => r.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var command = new UpdateVehicleCommand(vehicleId, "Toyota", "Camry", 2020, "White", 50000);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        vehicle.Brand.ShouldBe("Toyota");
        vehicle.Model.ShouldBe("Camry");
        vehicle.Year.ShouldBe(2020);
        vehicle.Color.ShouldBe("White");
        vehicle.Mileage.ShouldBe(50000);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenVehicleNotFound_ShouldReturnFailure()
    {
        // Arrange
        _vehicles.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "Toyota", "Camry", 2020, null, 0);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenVehicleBelongsToAnotherUser_ShouldReturnFailure()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var vehicle = Vehicle.Create("2T1BURHE0JC123456", "Toyota", "Camry", 2020, otherUserId);
        _vehicles.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "Ford", "Focus", 2018, null, 0);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUser.Setup(u => u.Id).Returns((Guid?)null);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "Toyota", "Camry", 2020, null, 0);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _vehicles.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
