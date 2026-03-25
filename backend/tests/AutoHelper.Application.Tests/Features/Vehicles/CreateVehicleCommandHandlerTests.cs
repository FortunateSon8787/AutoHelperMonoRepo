using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Vehicles.CreateVehicle;
using AutoHelper.Domain.Vehicles;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Vehicles;

public class CreateVehicleCommandHandlerTests
{
    private readonly Mock<IVehicleRepository> _vehicles = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreateVehicleCommandHandler _sut;

    private static readonly Guid UserId = Guid.NewGuid();

    public CreateVehicleCommandHandlerTests()
    {
        _sut = new CreateVehicleCommandHandler(
            _vehicles.Object,
            _currentUser.Object,
            _unitOfWork.Object);

        _currentUser.Setup(u => u.Id).Returns(UserId);
        _vehicles.Setup(r => r.ExistsByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    [Fact]
    public async Task Handle_WhenVinIsNew_ShouldCreateVehicleAndReturnId()
    {
        // Arrange
        var command = new CreateVehicleCommand("1HGCM82633A123456", "Honda", "Accord", 2003, null, 0);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WhenVinIsNew_ShouldCallRepositoryAddAndSaveChanges()
    {
        // Arrange
        var command = new CreateVehicleCommand("2T1BURHE0JC123456", "Toyota", "Camry", 2020, "Black", 15000);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert — vehicle must be added to the repository exactly once
        _vehicles.Verify(
            r => r.Add(It.Is<Vehicle>(v =>
                v.Vin == "2T1BURHE0JC123456" &&
                v.Brand == "Toyota" &&
                v.OwnerId == UserId)),
            Times.Once);

        // Assert — changes must be persisted exactly once
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenVinAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        _vehicles.Setup(r => r.ExistsByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateVehicleCommand("1HGCM82633A123456", "Honda", "Accord", 2003, null, 0);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeEmpty();

        _vehicles.Verify(r => r.Add(It.IsAny<Vehicle>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUser.Setup(u => u.Id).Returns((Guid?)null);

        var command = new CreateVehicleCommand("1HGCM82633A123456", "Honda", "Accord", 2003, null, 0);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _vehicles.Verify(r => r.Add(It.IsAny<Vehicle>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCreated_VehicleShouldBelongToCurrentUser()
    {
        // Arrange
        var command = new CreateVehicleCommand("1FTFW1ET5DKE12345", "Ford", "Focus", 2018, "Blue", 5000);
        Vehicle? capturedVehicle = null;
        _vehicles.Setup(r => r.Add(It.IsAny<Vehicle>()))
            .Callback<Vehicle>(v => capturedVehicle = v);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        capturedVehicle.ShouldNotBeNull();
        capturedVehicle.OwnerId.ShouldBe(UserId);
        capturedVehicle.Status.ShouldBe(VehicleStatus.Active);
    }
}
