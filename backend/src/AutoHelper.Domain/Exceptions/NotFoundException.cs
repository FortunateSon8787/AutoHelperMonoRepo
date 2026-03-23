namespace AutoHelper.Domain.Exceptions;

/// <summary>
/// Thrown when a required entity does not exist.
/// Maps to HTTP 404 Not Found at the API boundary.
/// </summary>
public sealed class NotFoundException(string entityName, object key)
    : Exception($"{entityName} with key '{key}' was not found.");
