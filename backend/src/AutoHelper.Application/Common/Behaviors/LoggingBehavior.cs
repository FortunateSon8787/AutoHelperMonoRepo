using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoHelper.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs each request with its execution time.
/// Helps trace slow handlers without touching business code.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Handling {RequestName}", requestName);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();

            if (IsFailedResult(response, out var error))
                logger.LogError(
                    "Handled {RequestName} with failure in {ElapsedMs}ms: {Error}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    error);
            else
                logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMs}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "Error handling {RequestName} after {ElapsedMs}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    private static bool IsFailedResult(TResponse response, out string? error)
    {
        if (response is Result result && result.IsFailure)
        {
            error = result.Error;
            return true;
        }

        // Result<TValue> has no common interface with Result, check via reflection-free pattern
        if (response is IFailureResult failureResult && failureResult.IsFailure)
        {
            error = failureResult.Error;
            return true;
        }

        error = null;
        return false;
    }
}
