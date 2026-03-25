using AutoHelper.Domain.Vehicles;
using Shouldly;

namespace AutoHelper.Domain.Tests.Vehicles;

public class VehicleStatusTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    private static Vehicle CreateVehicle() =>
        Vehicle.Create("1HGCM82633A123456", "Honda", "Accord", 2003, OwnerId);

    // ─── Active / ForSale ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(VehicleStatus.Active)]
    [InlineData(VehicleStatus.ForSale)]
    public void ChangeStatus_ToSimpleStatus_ShouldSucceedAndClearExtras(VehicleStatus status)
    {
        // Arrange
        var vehicle = CreateVehicle();

        // Act
        var result = vehicle.ChangeStatus(status, partnerName: null, documentUrl: null);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        vehicle.Status.ShouldBe(status);
        vehicle.PartnerName.ShouldBeNull();
        vehicle.DocumentUrl.ShouldBeNull();
    }

    // ─── InRepair ─────────────────────────────────────────────────────────────

    [Fact]
    public void ChangeStatus_ToInRepair_WithPartnerName_ShouldSucceed()
    {
        // Arrange
        var vehicle = CreateVehicle();

        // Act
        var result = vehicle.ChangeStatus(VehicleStatus.InRepair, partnerName: "AutoService Plus", documentUrl: null);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        vehicle.Status.ShouldBe(VehicleStatus.InRepair);
        vehicle.PartnerName.ShouldBe("AutoService Plus");
        vehicle.DocumentUrl.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeStatus_ToInRepair_WithoutPartnerName_ShouldFail(string? partnerName)
    {
        // Arrange
        var vehicle = CreateVehicle();

        // Act
        var result = vehicle.ChangeStatus(VehicleStatus.InRepair, partnerName: partnerName, documentUrl: null);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNullOrEmpty();
        vehicle.Status.ShouldBe(VehicleStatus.Active); // status unchanged
    }

    [Fact]
    public void ChangeStatus_ToInRepair_ShouldTrimPartnerName()
    {
        // Arrange
        var vehicle = CreateVehicle();

        // Act
        vehicle.ChangeStatus(VehicleStatus.InRepair, partnerName: "  AutoFix  ", documentUrl: null);

        // Assert
        vehicle.PartnerName.ShouldBe("AutoFix");
    }

    // ─── Recycled / Dismantled ────────────────────────────────────────────────

    [Theory]
    [InlineData(VehicleStatus.Recycled)]
    [InlineData(VehicleStatus.Dismantled)]
    public void ChangeStatus_ToRecycledOrDismantled_WithDocumentUrl_ShouldSucceed(VehicleStatus status)
    {
        // Arrange
        var vehicle = CreateVehicle();
        const string docUrl = "https://storage/vehicles/documents/cert.pdf";

        // Act
        var result = vehicle.ChangeStatus(status, partnerName: null, documentUrl: docUrl);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        vehicle.Status.ShouldBe(status);
        vehicle.DocumentUrl.ShouldBe(docUrl);
        vehicle.PartnerName.ShouldBeNull();
    }

    [Theory]
    [InlineData(VehicleStatus.Recycled, null)]
    [InlineData(VehicleStatus.Recycled, "")]
    [InlineData(VehicleStatus.Recycled, "   ")]
    [InlineData(VehicleStatus.Dismantled, null)]
    [InlineData(VehicleStatus.Dismantled, "")]
    public void ChangeStatus_ToRecycledOrDismantled_WithoutDocumentUrl_ShouldFail(
        VehicleStatus status, string? documentUrl)
    {
        // Arrange
        var vehicle = CreateVehicle();

        // Act
        var result = vehicle.ChangeStatus(status, partnerName: null, documentUrl: documentUrl);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNullOrEmpty();
        vehicle.Status.ShouldBe(VehicleStatus.Active); // status unchanged
    }

    // ─── Transitions clear stale data ─────────────────────────────────────────

    [Fact]
    public void ChangeStatus_FromInRepairToActive_ShouldClearPartnerName()
    {
        // Arrange
        var vehicle = CreateVehicle();
        vehicle.ChangeStatus(VehicleStatus.InRepair, partnerName: "AutoFix", documentUrl: null);

        // Act
        vehicle.ChangeStatus(VehicleStatus.Active, partnerName: null, documentUrl: null);

        // Assert
        vehicle.Status.ShouldBe(VehicleStatus.Active);
        vehicle.PartnerName.ShouldBeNull();
    }

    [Fact]
    public void ChangeStatus_FromRecycledToActive_ShouldClearDocumentUrl()
    {
        // Arrange
        var vehicle = CreateVehicle();
        vehicle.ChangeStatus(VehicleStatus.Recycled, partnerName: null, documentUrl: "https://s3/doc.pdf");

        // Act
        vehicle.ChangeStatus(VehicleStatus.Active, partnerName: null, documentUrl: null);

        // Assert
        vehicle.Status.ShouldBe(VehicleStatus.Active);
        vehicle.DocumentUrl.ShouldBeNull();
    }
}
