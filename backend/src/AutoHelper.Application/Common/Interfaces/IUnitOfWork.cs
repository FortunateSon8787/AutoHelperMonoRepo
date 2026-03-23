namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Abstracts the transaction boundary.
/// Handlers call SaveChangesAsync to persist all changes within a single transaction.
/// Domain events are dispatched after SaveChangesAsync.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
