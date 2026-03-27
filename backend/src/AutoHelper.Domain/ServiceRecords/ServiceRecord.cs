using AutoHelper.Domain.Common;
using AutoHelper.Domain.Exceptions;
using AutoHelper.Domain.ServiceRecords.Events;

namespace AutoHelper.Domain.ServiceRecords;

/// <summary>
/// Aggregate root representing a service record for a vehicle.
/// Each record must have a PDF work order document attached.
/// </summary>
public sealed class ServiceRecord : AggregateRoot<Guid>
{
    /// <summary>FK to the vehicle this record belongs to.</summary>
    public Guid VehicleId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    /// <summary>When the service work was actually performed (UTC).</summary>
    public DateTime PerformedAt { get; private set; }

    public decimal Cost { get; private set; }

    /// <summary>Name of the partner or third-party organization that performed the work.</summary>
    public string ExecutorName { get; private set; } = string.Empty;

    /// <summary>Contact details for the executor (phone, address, etc.).</summary>
    public string? ExecutorContacts { get; private set; }

    /// <summary>List of individual operations performed. Stored as JSON in the database.</summary>
    public List<string> Operations { get; private set; } = [];

    /// <summary>URL to the PDF work order document. Required and immutable after creation.</summary>
    public string DocumentUrl { get; private set; } = string.Empty;

    public bool IsDeleted { get; private set; }

    // ─── EF Core ──────────────────────────────────────────────────────────────

    private ServiceRecord() { }

    // ─── Factory method ───────────────────────────────────────────────────────

    public static ServiceRecord Create(
        Guid vehicleId,
        string title,
        string description,
        DateTime performedAt,
        decimal cost,
        string executorName,
        string? executorContacts,
        List<string> operations,
        string documentUrl)
    {
        if (string.IsNullOrWhiteSpace(documentUrl))
            throw new DomainException("A service record must have a PDF document (documentUrl is required).");

        var record = new ServiceRecord
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            Title = title.Trim(),
            Description = description.Trim(),
            PerformedAt = performedAt,
            Cost = cost,
            ExecutorName = executorName.Trim(),
            ExecutorContacts = executorContacts?.Trim(),
            Operations = operations,
            DocumentUrl = documentUrl.Trim(),
            IsDeleted = false
        };

        record.AddDomainEvent(new ServiceRecordCreatedEvent(record.Id, vehicleId));

        return record;
    }

    // ─── Business operations ──────────────────────────────────────────────────

    /// <summary>
    /// Updates mutable fields. DocumentUrl is intentionally excluded — the PDF is immutable.
    /// </summary>
    public void Update(
        string title,
        string description,
        DateTime performedAt,
        decimal cost,
        string executorName,
        string? executorContacts,
        List<string> operations)
    {
        Title = title.Trim();
        Description = description.Trim();
        PerformedAt = performedAt;
        Cost = cost;
        ExecutorName = executorName.Trim();
        ExecutorContacts = executorContacts?.Trim();
        Operations = operations;
    }

    /// <summary>Soft-deletes the record. Physical deletion from the database is prohibited.</summary>
    public void Delete() => IsDeleted = true;
}
