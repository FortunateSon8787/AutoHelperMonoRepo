using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.ServiceRecords;
using Microsoft.EntityFrameworkCore;

namespace AutoHelper.Infrastructure.Persistence.Repositories;

public sealed class ServiceRecordRepository(AppDbContext context) : IServiceRecordRepository
{
    public Task<ServiceRecord?> GetByIdAsync(Guid id, CancellationToken ct) =>
        context.ServiceRecords
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<ServiceRecord>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken ct)
    {
        var records = await context.ServiceRecords
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.PerformedAt)
            .ToListAsync(ct);

        return records;
    }

    public void Add(ServiceRecord record) =>
        context.ServiceRecords.Add(record);
}
