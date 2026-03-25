using AutoHelper.Domain.Exceptions;
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
        var vehicle = Vehicle.Create("2T1BURHE0JC123456", "BMW", "X5", 2022, OwnerId, color: "Black", mileage: 15000);

        // Assert
        vehicle.Color.ShouldBe("Black");
        vehicle.Mileage.ShouldBe(15000);
    }

    [Fact]
    public void Create_ShouldDefaultStatusToActive()
    {
        // Act
        var vehicle = Vehicle.Create("1FTFW1ET5DKE12345", "Ford", "Focus", 2018, OwnerId);

        // Assert
        vehicle.Status.ShouldBe(VehicleStatus.Active);
    }

    [Fact]
    public void Create_TwoVehicles_ShouldHaveDifferentIds()
    {
        // Act
        var v1 = Vehicle.Create("1FTFW1ET5DKE12345", "Ford", "Focus", 2018, OwnerId);
        var v2 = Vehicle.Create("1FTFW1ET5DKE12346", "Ford", "Focus", 2018, OwnerId);

        // Assert
        v1.Id.ShouldNotBe(v2.Id);
    }

    // ─── VIN validation ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]                    // empty
    [InlineData("1HGCM82633A12345")]    // 16 chars (too short)
    [InlineData("1HGCM82633A1234567")]  // 18 chars (too long)
    [InlineData("1HGCM82633I123456")]   // contains I
    [InlineData("1HGCM82633O123456")]   // contains O
    [InlineData("1HGCM82633Q123456")]   // contains Q
    [InlineData("1HGCM82633A12345!")]   // special character
    public void Create_WithInvalidVin_ShouldThrowDomainException(string invalidVin)
    {
        Should.Throw<DomainException>(() =>
            Vehicle.Create(invalidVin, "Honda", "Accord", 2003, OwnerId));
    }

    // ─── UpdateDetails ────────────────────────────────────────────────────────

    [Fact]
    public void UpdateDetails_ShouldUpdateMutableFields()
    {
        // Arrange
        var vehicle = Vehicle.Create("1HGCM82633A123456", "Honda", "Accord", 2003, OwnerId);

        // Act
        vehicle.UpdateDetails("Toyota", "Camry", 2020, "White", 50000);

        // Assert
        vehicle.Brand.ShouldBe("Toyota");
        vehicle.Model.ShouldBe("Camry");
        vehicle.Year.ShouldBe(2020);
        vehicle.Color.ShouldBe("White");
        vehicle.Mileage.ShouldBe(50000);
    }

    [Fact]
    public void UpdateDetails_ShouldTrimBrandAndModel()
    {
        // Arrange
        var vehicle = Vehicle.Create("2T1BURHE0JC123456", "BMW", "X5", 2022, OwnerId);

        // Act
        vehicle.UpdateDetails("  Ford  ", "  Focus  ", 2019, null, 0);

        // Assert
        vehicle.Brand.ShouldBe("Ford");
        vehicle.Model.ShouldBe("Focus");
    }

    [Fact]
    public void UpdateDetails_ShouldNotChangeVinOrStatus()
    {
        // Arrange
        var vehicle = Vehicle.Create("1HGCM82633A123456", "Honda", "Accord", 2003, OwnerId);

        // Act
        vehicle.UpdateDetails("Toyota", "Camry", 2020, null, 0);

        // Assert — immutable fields unchanged
        vehicle.Vin.ShouldBe("1HGCM82633A123456");
        vehicle.Status.ShouldBe(VehicleStatus.Active);
        vehicle.OwnerId.ShouldBe(OwnerId);
    }

    [Fact]
    public void UpdateDetails_WithNullColor_ShouldSetColorToNull()
    {
        // Arrange
        var vehicle = Vehicle.Create("1FTFW1ET5DKE12345", "BMW", "X5", 2022, OwnerId, color: "Black");

        // Act
        vehicle.UpdateDetails("BMW", "X5", 2022, null, 0);

        // Assert
        vehicle.Color.ShouldBeNull();
    }
}
