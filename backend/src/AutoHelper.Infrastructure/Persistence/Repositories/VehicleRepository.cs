using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace AutoHelper.Infrastructure.Persistence.Repositories;

public sealed class VehicleRepository(AppDbContext db) : IVehicleRepository
{
    public Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Vehicles.FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<Vehicle?> GetByVinAsync(string vin, CancellationToken ct) =>
        db.Vehicles
            .FirstOrDefaultAsync(v => v.Vin == vin.Trim().ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<Vehicle>> GetAllByOwnerIdAsync(Guid ownerId, CancellationToken ct) =>
        await db.Vehicles.Where(v => v.OwnerId == ownerId).ToListAsync(ct);

    public Task<bool> ExistsByVinAsync(string vin, CancellationToken ct) =>
        db.Vehicles
            .AnyAsync(v => v.Vin == vin.Trim().ToUpperInvariant(), ct);

    public void Add(Vehicle vehicle) =>
        db.Vehicles.Add(vehicle);
}
