namespace AutoHelper.Domain.Common;

/// <summary>
/// Represents the outcome of a domain operation that can fail expectedly.
/// Prefer over exceptions for known business rule violations.
/// </summary>
public sealed class Result
{
    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
}
