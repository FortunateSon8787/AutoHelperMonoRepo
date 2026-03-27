using AutoHelper.Domain.Common;

namespace AutoHelper.Domain.ServiceRecords.Events;

/// <summary>
/// Raised when a new service record is added to a vehicle's history.
/// </summary>
public sealed record ServiceRecordCreatedEvent(
    Guid ServiceRecordId,
    Guid VehicleId) : IDomainEvent;
