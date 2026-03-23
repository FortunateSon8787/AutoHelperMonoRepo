namespace AutoHelper.Application.Common;

/// <summary>
/// Represents the outcome of an operation that can either succeed or fail expectedly.
/// Use for business rule failures in Application layer handlers.
/// Use exceptions for unexpected/infrastructure failures.
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

    public static implicit operator Result(string error) => Failure(error);
}

/// <summary>
/// Represents the outcome of an operation that returns a value on success.
/// </summary>
public sealed class Result<TValue>
{
    private readonly TValue? _value;

    private Result(TValue value)
    {
        IsSuccess = true;
        _value = value;
        Error = null;
    }

    private Result(string error)
    {
        IsSuccess = false;
        _value = default;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of a failed Result.");

    public static Result<TValue> Success(TValue value) => new(value);
    public static Result<TValue> Failure(string error) => new(error);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(string error) => Failure(error);
}
