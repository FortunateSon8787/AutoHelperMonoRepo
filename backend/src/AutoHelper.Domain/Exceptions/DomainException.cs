namespace AutoHelper.Domain.Exceptions;

/// <summary>
/// Thrown when a domain invariant is violated.
/// Maps to HTTP 400 Bad Request at the API boundary.
/// </summary>
public sealed class DomainException(string message) : Exception(message);
