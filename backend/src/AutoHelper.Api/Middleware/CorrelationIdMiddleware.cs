using Serilog.Context;

namespace AutoHelper.Api.Middleware;

/// <summary>
/// Ensures every request carries an X-Correlation-Id header.
/// If the client sends one it is re-used; otherwise a new short ID is generated.
/// The ID is:
///   • pushed onto Serilog's LogContext so every log line inside the request carries it,
///   • stored in HttpContext.Items for downstream use,
///   • echoed back to the client in the response headers.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";
    public const string HttpContextItemKey = "CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
            ?? GenerateId();

        context.Items[HttpContextItemKey] = correlationId;

        // Echo the ID in the response before headers are flushed
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // Push into Serilog LogContext — visible on every log entry in this request
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }

    private static string GenerateId() => Guid.NewGuid().ToString("N")[..12];
}
