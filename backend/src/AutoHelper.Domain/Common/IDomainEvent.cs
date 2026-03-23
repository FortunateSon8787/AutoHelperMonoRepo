using MediatR;

namespace AutoHelper.Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// Domain events are raised inside aggregate roots and dispatched after SaveChanges.
/// </summary>
public interface IDomainEvent : INotification;
