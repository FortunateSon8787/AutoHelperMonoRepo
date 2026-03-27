using AutoHelper.Domain.ServiceRecords;

namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Provides data access operations for the ServiceRecord aggregate.
/// </summary>
public interface IServiceRecordRepository
{
    /// <summary>Finds a service record by its primary key. Returns null if not found or soft-deleted.</summary>
    Task<ServiceRecord?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Returns all non-deleted service records for a given vehicle, ordered by PerformedAt descending.</summary>
    Task<IReadOnlyList<ServiceRecord>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken ct);

    /// <summary>Adds a new service record (tracked, not yet persisted).</summary>
    void Add(ServiceRecord record);
}
