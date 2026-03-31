namespace AutoHelper.Application.Common;

/// <summary>
/// Non-generic interface implemented by both Result and Result&lt;T&gt;.
/// Allows LoggingBehavior to detect failures without reflection or generics.
/// </summary>
public interface IFailureResult
{
    bool IsFailure { get; }
    AppError? Error { get; }
}

/// <summary>
/// Represents the outcome of an operation that can either succeed or fail expectedly.
/// Use for business rule failures in Application layer handlers.
/// Use exceptions for unexpected/infrastructure failures.
/// </summary>
public sealed class Result : IFailureResult
{
    private Result(bool isSuccess, AppError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public AppError? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(AppError error) => new(false, error);

    public static implicit operator Result(AppError error) => Failure(error);
}

/// <summary>
/// Represents the outcome of an operation that returns a value on success.
/// </summary>
public sealed class Result<TValue> : IFailureResult
{
    private readonly TValue? _value;

    private Result(TValue value)
    {
        IsSuccess = true;
        _value = value;
        Error = null;
    }

    private Result(AppError error)
    {
        IsSuccess = false;
        _value = default;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public AppError? Error { get; }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of a failed Result.");

    public static Result<TValue> Success(TValue value) => new(value);
    public static Result<TValue> Failure(AppError error) => new(error);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(AppError error) => Failure(error);
}
