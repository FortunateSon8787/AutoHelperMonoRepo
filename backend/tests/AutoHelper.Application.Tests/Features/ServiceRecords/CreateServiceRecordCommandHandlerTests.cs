using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.ServiceRecords;
using AutoHelper.Application.Features.ServiceRecords.CreateServiceRecord;
using AutoHelper.Domain.ServiceRecords;
using AutoHelper.Domain.Vehicles;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.ServiceRecords;

public class CreateServiceRecordCommandHandlerTests
{
    private readonly Mock<IServiceRecordRepository> _serviceRecords = new();
    private readonly Mock<IVehicleRepository> _vehicles = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreateServiceRecordCommandHandler _sut;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid VehicleId = Guid.NewGuid();
    private static readonly List<string> ValidOperations = ["Oil change"];
    private const string ValidDocumentUrl = "https://r2.example.com/docs/123.pdf";

    public CreateServiceRecordCommandHandlerTests()
    {
        _sut = new CreateServiceRecordCommandHandler(
            _serviceRecords.Object,
            _vehicles.Object,
            _currentUser.Object,
            _unitOfWork.Object);

        _currentUser.Setup(u => u.Id).Returns(UserId);

        var vehicle = Vehicle.Create("1HGCM82633A123456", "Honda", "Accord", 2003, UserId);
        SetVehicleId(vehicle, VehicleId);

        _vehicles.Setup(r => r.GetByIdAsync(VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateRecordAndReturnId()
    {
        // Arrange
        var command = ValidCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallRepositoryAddAndSave()
    {
        // Arrange
        var command = ValidCommand();
        ServiceRecord? captured = null;
        _serviceRecords.Setup(r => r.Add(It.IsAny<ServiceRecord>()))
            .Callback<ServiceRecord>(r => captured = r);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert — repository.Add must be called exactly once with correct data
        _serviceRecords.Verify(
            r => r.Add(It.Is<ServiceRecord>(sr =>
                sr.VehicleId == VehicleId &&
                sr.Title == command.Title &&
                sr.DocumentUrl == command.DocumentUrl)),
            Times.Once);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        captured.ShouldNotBeNull();
        captured.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUser.Setup(u => u.Id).Returns((Guid?)null);

        // Act
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _serviceRecords.Verify(r => r.Add(It.IsAny<ServiceRecord>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenVehicleNotFound_ShouldReturnFailure()
    {
        // Arrange
        _vehicles.Setup(r => r.GetByIdAsync(VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        // Act
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _serviceRecords.Verify(r => r.Add(It.IsAny<ServiceRecord>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenVehicleBelongsToDifferentUser_ShouldReturnFailure()
    {
        // Arrange — vehicle owned by a different user
        var otherOwnerId = Guid.NewGuid();
        var foreignVehicle = Vehicle.Create("1HGCM82633A123456", "Honda", "Accord", 2003, otherOwnerId);
        SetVehicleId(foreignVehicle, VehicleId);

        _vehicles.Setup(r => r.GetByIdAsync(VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(foreignVehicle);

        // Act
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error!.ShouldContain("Access denied");
        _serviceRecords.Verify(r => r.Add(It.IsAny<ServiceRecord>()), Times.Never);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static CreateServiceRecordCommand ValidCommand() =>
        new(VehicleId, "Oil change", "Replaced engine oil and filter",
            DateTime.UtcNow.AddDays(-1), 150m, "AutoService Pro", "+7-999-000-0000",
            ValidOperations, ValidDocumentUrl);

    /// <summary>
    /// Workaround: Vehicle.Id is set via Guid.NewGuid() inside the factory method.
    /// We use reflection to override it for test isolation.
    /// </summary>
    private static void SetVehicleId(Vehicle vehicle, Guid id)
    {
        var prop = typeof(AutoHelper.Domain.Common.Entity<Guid>)
            .GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        prop?.SetValue(vehicle, id);
    }
}
