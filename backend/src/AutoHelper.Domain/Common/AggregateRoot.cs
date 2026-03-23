namespace AutoHelper.Domain.Common;

/// <summary>
/// Base class for aggregate roots.
/// Aggregate roots are the entry point to a consistency boundary.
/// Domain events are raised here and dispatched after the transaction commits.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    protected AggregateRoot(TId id) : base(id) { }

    // Required for EF Core
    protected AggregateRoot() { }
}
