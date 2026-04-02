namespace AutoHelper.Application.Common;

/// <summary>
/// Classifies the nature of an error for HTTP status code mapping.
/// </summary>
public enum ErrorType
{
    /// <summary>Business rule violation or invalid input — maps to 400 Bad Request.</summary>
    Validation,

    /// <summary>Requested resource was not found — maps to 404 Not Found.</summary>
    NotFound,

    /// <summary>Operation not permitted for the current user — maps to 403 Forbidden.</summary>
    Forbidden,

    /// <summary>Authentication failed — maps to 401 Unauthorized.</summary>
    Unauthorized,

    /// <summary>Conflict with existing resource state — maps to 409 Conflict.</summary>
    Conflict
}

/// <summary>
/// Represents a structured application error with a machine-readable code and a human-readable description.
/// All error instances are defined in <see cref="AppErrors"/> — do not create ad-hoc errors.
/// </summary>
public sealed record AppError(string Code, string Description, ErrorType Type = ErrorType.Validation)
{
    /// <summary>Returns the appropriate HTTP status code for this error type.</summary>
    public int ToHttpStatusCode() => Type switch
    {
        ErrorType.NotFound     => 404,
        ErrorType.Forbidden    => 403,
        ErrorType.Unauthorized => 401,
        ErrorType.Conflict     => 409,
        _                      => 400
    };

    public override string ToString() => $"{Code}: {Description}";
}
