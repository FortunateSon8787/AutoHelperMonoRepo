using AutoHelper.Domain.Vehicles;
using Shouldly;

namespace AutoHelper.Domain.Tests.Vehicles;

public class VehicleTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    // ─── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ShouldCreateVehicle()
    {
        // Act
        var vehicle = Vehicle.Create("1HGCM82633A123456", "Honda", "Accord", 2003, OwnerId);

        // Assert
        vehicle.Id.ShouldNotBe(Guid.Empty);
        vehicle.Vin.ShouldBe("1HGCM82633A123456");
        vehicle.Brand.ShouldBe("Honda");
        vehicle.Model.ShouldBe("Accord");
        vehicle.Year.ShouldBe(2003);
        vehicle.OwnerId.ShouldBe(OwnerId);
        vehicle.Status.ShouldBe(VehicleStatus.Active);
        vehicle.Mileage.ShouldBe(0);
        vehicle.Color.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldNormalizeVinToUpperCase()
    {
        // Act
        var vehicle = Vehicle.Create("  1hgcm82633a123456  ", "Toyota", "Camry", 2020, OwnerId);

        // Assert
        vehicle.Vin.ShouldBe("1HGCM82633A123456");
    }

    [Fact]
    public void Create_WithOptionalFields_ShouldSetThem()
    {
        // Act
        var vehicle = Vehicle.Create("VIN123456789", "BMW", "X5", 2022, OwnerId, color: "Black", mileage: 15000);

        // Assert
        vehicle.Color.ShouldBe("Black");
        vehicle.Mileage.ShouldBe(15000);
    }

    [Fact]
    public void Create_ShouldDefaultStatusToActive()
    {
        // Act
        var vehicle = Vehicle.Create("VIN000000001", "Ford", "Focus", 2018, OwnerId);

        // Assert
        vehicle.Status.ShouldBe(VehicleStatus.Active);
    }

    [Fact]
    public void Create_TwoVehicles_ShouldHaveDifferentIds()
    {
        // Act
        var v1 = Vehicle.Create("VIN000000001", "Ford", "Focus", 2018, OwnerId);
        var v2 = Vehicle.Create("VIN000000002", "Ford", "Focus", 2018, OwnerId);

        // Assert
        v1.Id.ShouldNotBe(v2.Id);
    }
}
