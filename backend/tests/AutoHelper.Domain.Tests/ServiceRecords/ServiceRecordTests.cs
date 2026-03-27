using AutoHelper.Domain.Exceptions;
using AutoHelper.Domain.ServiceRecords;
using AutoHelper.Domain.ServiceRecords.Events;
using Shouldly;

namespace AutoHelper.Domain.Tests.ServiceRecords;

public class ServiceRecordTests
{
    private static readonly Guid VehicleId = Guid.NewGuid();
    private static readonly List<string> ValidOperations = ["Oil change", "Filter replacement"];
    private const string ValidDocumentUrl = "https://storage.example.com/docs/123.pdf";

    // ─── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ShouldCreateRecord()
    {
        // Act
        var record = ServiceRecord.Create(
            vehicleId: VehicleId,
            title: "Routine maintenance",
            description: "Full oil change and filter replacement",
            performedAt: new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc),
            cost: 150.00m,
            executorName: "AutoService Pro",
            executorContacts: "+7-999-000-0000",
            operations: ValidOperations,
            documentUrl: ValidDocumentUrl);

        // Assert
        record.Id.ShouldNotBe(Guid.Empty);
        record.VehicleId.ShouldBe(VehicleId);
        record.Title.ShouldBe("Routine maintenance");
        record.Description.ShouldBe("Full oil change and filter replacement");
        record.Cost.ShouldBe(150.00m);
        record.ExecutorName.ShouldBe("AutoService Pro");
        record.ExecutorContacts.ShouldBe("+7-999-000-0000");
        record.Operations.ShouldBe(ValidOperations);
        record.DocumentUrl.ShouldBe(ValidDocumentUrl);
        record.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Create_TwoRecords_ShouldHaveDifferentIds()
    {
        // Act
        var r1 = ServiceRecord.Create(VehicleId, "Title 1", "Desc", DateTime.UtcNow.AddDays(-1), 100m, "Exec", null, ValidOperations, ValidDocumentUrl);
        var r2 = ServiceRecord.Create(VehicleId, "Title 2", "Desc", DateTime.UtcNow.AddDays(-2), 200m, "Exec", null, ValidOperations, ValidDocumentUrl);

        // Assert
        r1.Id.ShouldNotBe(r2.Id);
    }

    [Fact]
    public void Create_ShouldRaiseServiceRecordCreatedEvent()
    {
        // Act
        var record = ServiceRecord.Create(VehicleId, "Title", "Desc", DateTime.UtcNow.AddDays(-1), 0m, "Exec", null, ValidOperations, ValidDocumentUrl);

        // Assert
        record.DomainEvents.Count.ShouldBe(1);
        var evt = record.DomainEvents.First().ShouldBeOfType<ServiceRecordCreatedEvent>();
        evt.ServiceRecordId.ShouldBe(record.Id);
        evt.VehicleId.ShouldBe(VehicleId);
    }

    [Fact]
    public void Create_ShouldTrimTitleAndDescription()
    {
        // Act
        var record = ServiceRecord.Create(VehicleId, "  Title  ", "  Desc  ", DateTime.UtcNow.AddDays(-1), 0m, "  Exec  ", null, ValidOperations, ValidDocumentUrl);

        // Assert
        record.Title.ShouldBe("Title");
        record.Description.ShouldBe("Desc");
        record.ExecutorName.ShouldBe("Exec");
    }

    [Fact]
    public void Create_WithNullExecutorContacts_ShouldBeAllowed()
    {
        // Act
        var record = ServiceRecord.Create(VehicleId, "Title", "Desc", DateTime.UtcNow.AddDays(-1), 0m, "Exec", null, ValidOperations, ValidDocumentUrl);

        // Assert
        record.ExecutorContacts.ShouldBeNull();
    }

    // ─── DocumentUrl invariant ────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDocumentUrl_ShouldThrowDomainException(string emptyUrl)
    {
        Should.Throw<DomainException>(() =>
            ServiceRecord.Create(VehicleId, "Title", "Desc", DateTime.UtcNow.AddDays(-1), 0m, "Exec", null, ValidOperations, emptyUrl))
            .Message.ShouldContain("documentUrl");
    }

    // ─── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public void Update_ShouldChangeMutableFields()
    {
        // Arrange
        var record = ServiceRecord.Create(VehicleId, "Old title", "Old desc", DateTime.UtcNow.AddDays(-10), 100m, "Old Exec", null, ValidOperations, ValidDocumentUrl);

        // Act
        record.Update(
            title: "New title",
            description: "New desc",
            performedAt: DateTime.UtcNow.AddDays(-5),
            cost: 250m,
            executorName: "New Exec",
            executorContacts: "contact@new.com",
            operations: ["New operation"]);

        // Assert
        record.Title.ShouldBe("New title");
        record.Description.ShouldBe("New desc");
        record.Cost.ShouldBe(250m);
        record.ExecutorName.ShouldBe("New Exec");
        record.ExecutorContacts.ShouldBe("contact@new.com");
        record.Operations.ShouldContain("New operation");
    }

    [Fact]
    public void Update_ShouldNotChangeDocumentUrl()
    {
        // Arrange
        var originalUrl = "https://storage.example.com/original.pdf";
        var record = ServiceRecord.Create(VehicleId, "Title", "Desc", DateTime.UtcNow.AddDays(-1), 0m, "Exec", null, ValidOperations, originalUrl);

        // Act
        record.Update("New title", "New desc", DateTime.UtcNow.AddDays(-1), 0m, "New Exec", null, ValidOperations);

        // Assert — DocumentUrl must remain unchanged after update
        record.DocumentUrl.ShouldBe(originalUrl);
    }

    [Fact]
    public void Update_ShouldNotChangeVehicleIdOrId()
    {
        // Arrange
        var record = ServiceRecord.Create(VehicleId, "Title", "Desc", DateTime.UtcNow.AddDays(-1), 0m, "Exec", null, ValidOperations, ValidDocumentUrl);
        var originalId = record.Id;

        // Act
        record.Update("New title", "New desc", DateTime.UtcNow.AddDays(-1), 0m, "New Exec", null, ValidOperations);

        // Assert
        record.Id.ShouldBe(originalId);
        record.VehicleId.ShouldBe(VehicleId);
    }

    // ─── Delete (soft-delete) ─────────────────────────────────────────────────

    [Fact]
    public void Delete_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var record = ServiceRecord.Create(VehicleId, "Title", "Desc", DateTime.UtcNow.AddDays(-1), 0m, "Exec", null, ValidOperations, ValidDocumentUrl);
        record.IsDeleted.ShouldBeFalse();

        // Act
        record.Delete();

        // Assert
        record.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void Delete_ShouldNotClearOtherFields()
    {
        // Arrange
        var record = ServiceRecord.Create(VehicleId, "Title", "Desc", DateTime.UtcNow.AddDays(-1), 100m, "Exec", null, ValidOperations, ValidDocumentUrl);

        // Act
        record.Delete();

        // Assert — only IsDeleted changes
        record.Title.ShouldBe("Title");
        record.DocumentUrl.ShouldBe(ValidDocumentUrl);
        record.Cost.ShouldBe(100m);
    }
}
