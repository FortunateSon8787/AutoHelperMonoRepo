using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Customers;
using AutoHelper.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoHelper.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository(AppDbContext db) : ICustomerRepository
{
    public Task<Customer?> GetByEmailAsync(string email, CancellationToken ct) =>
        db.Customers
            .FirstOrDefaultAsync(c => c.Email == email.Trim().ToLowerInvariant(), ct);

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Customers.FindAsync([id], ct).AsTask();

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        db.Customers
            .AnyAsync(c => c.Email == email.Trim().ToLowerInvariant(), ct);

    public void Add(Customer customer) =>
        db.Customers.Add(customer);

    public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, CancellationToken ct)
    {
        var query = db.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Email.Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .AsNoTracking()
            .OrderByDescending(c => c.RegistrationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
