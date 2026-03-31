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

    public async Task<(IReadOnlyList<Vehicle> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, CancellationToken ct)
    {
        var query = db.Vehicles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
            query = query.Where(v =>
                v.Vin.Contains(term) ||
                v.Brand.ToUpper().Contains(term) ||
                v.Model.ToUpper().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(v => v.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
