using AutoHelper.Domain.Common;

namespace AutoHelper.Domain.Customers.Events;

/// <summary>
/// Raised when a new customer registers in the system.
/// Can be used to trigger welcome emails, analytics events, etc.
/// </summary>
public sealed record CustomerRegisteredEvent(Guid CustomerId, string Email) : IDomainEvent;
