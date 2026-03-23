using AutoHelper.Domain.Customers;

namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Provides data access operations for the Customer aggregate.
/// </summary>
public interface ICustomerRepository
{
    /// <summary>Finds a customer by their primary email address.</summary>
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct);

    /// <summary>Finds a customer by their unique identifier.</summary>
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Checks whether an email address is already registered.</summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);

    /// <summary>Adds a new customer to the repository (tracked, not yet persisted).</summary>
    void Add(Customer customer);
}
