using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Vehicles.ChangeVehicleStatus;
using AutoHelper.Domain.Vehicles;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Vehicles;

public class ChangeVehicleStatusCommandHandlerTests
{
    private readonly Mock<IVehicleRepository> _vehicles = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ChangeVehicleStatusCommandHandler _sut;

    private static readonly Guid UserId = Guid.NewGuid();

    public ChangeVehicleStatusCommandHandlerTests()
    {
        _sut = new ChangeVehicleStatusCommandHandler(
            _vehicles.Object,
            _currentUser.Object,
            _unitOfWork.Object);

        _currentUser.Setup(u => u.Id).Returns(UserId);
    }

    [Fact]
    public async Task Handle_WhenChangingToForSale_ShouldSucceedAndSave()
    {
        // Arrange
        var vehicle = Vehicle.Create("1HGCM82633A123456", "Honda", "Accord", 2003, UserId);
        _vehicles.Setup(r => r.GetByIdAsync(vehicle.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var command = new ChangeVehicleStatusCommand(vehicle.Id, VehicleStatus.ForSale, null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        vehicle.Status.ShouldBe(VehicleStatus.ForSale);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenChangingToInRepairWithPartnerName_ShouldSucceed()
    {
        // Arrange
        var vehicle = Vehicle.Create("1HGCM82633A123456", "Honda", "Accord", 2003, UserId);
        _vehicles.Setup(r => r.GetByIdAsync(vehicle.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var command = new ChangeVehicleStatusCommand(vehicle.Id, VehicleStatus.InRepair, "AutoService Plus", null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        vehicle.Status.ShouldBe(VehicleStatus.InRepair);
        vehicle.PartnerName.ShouldBe("AutoService Plus");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenChangingToInRepairWithoutPartnerName_ShouldReturnFailureAndNotSave()
    {
        // Arrange
        var vehicle = Vehicle.Create("1HGCM82633A123456", "Honda", "Accord", 2003, UserId);
        _vehicles.Setup(r => r.GetByIdAsync(vehicle.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var command = new ChangeVehicleStatusCommand(vehicle.Id, VehicleStatus.InRepair, null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenChangingToRecycledWithDocumentUrl_ShouldSucceed()
    {
        // Arrange
        var vehicle = Vehicle.Create("1HGCM82633A123456", "Honda", "Accord", 2003, UserId);
        _vehicles.Setup(r => r.GetByIdAsync(vehicle.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        const string docUrl = "https://storage/vehicles/documents/cert.pdf";
        var command = new ChangeVehicleStatusCommand(vehicle.Id, VehicleStatus.Recycled, null, docUrl);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        vehicle.Status.ShouldBe(VehicleStatus.Recycled);
        vehicle.DocumentUrl.ShouldBe(docUrl);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenVehicleNotFound_ShouldReturnFailure()
    {
        // Arrange
        _vehicles.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        var command = new ChangeVehicleStatusCommand(Guid.NewGuid(), VehicleStatus.ForSale, null, null);

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

        var command = new ChangeVehicleStatusCommand(vehicle.Id, VehicleStatus.ForSale, null, null);

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

        var command = new ChangeVehicleStatusCommand(Guid.NewGuid(), VehicleStatus.ForSale, null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _vehicles.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
