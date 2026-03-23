using System.Diagnostics;
using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Context;
using ILogger = Serilog.ILogger;

namespace AutoHelper.Api.Middleware;

/// <summary>
/// Structured HTTP request/response logger.
///
/// On every request it logs:
///   • HTTP method, path, query string
///   • Sanitised request headers (sensitive values are masked)
///   • Response status code
///   • Elapsed milliseconds
///
/// Sensitive headers that are MASKED (value replaced with ***):
///   Authorization, Cookie, Set-Cookie, X-Api-Key, X-Auth-Token
///
/// Configuration (appsettings.json):
///   Logging:Http:Enabled          — enable/disable this middleware (default: true)
///   Logging:Http:LogRequestHeaders — include request headers in the log (default: true)
/// </summary>
public sealed class HttpLoggingMiddleware(
    RequestDelegate next,
    IConfiguration configuration)
{
    private static readonly ILogger Log = Serilog.Log.ForContext<HttpLoggingMiddleware>();

    // Headers whose values must never appear in plain text in logs
    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "X-Api-Key",
        "X-Auth-Token",
        "Proxy-Authorization",
    };

    private const string MaskedValue = "***";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsEnabled())
        {
            await next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        var correlationId = context.Items[CorrelationIdMiddleware.HttpContextItemKey]?.ToString() ?? "-";
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "/";
        var query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;

        try
        {
            await next(context);
        }
        finally
        {
            sw.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsed = sw.ElapsedMilliseconds;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("HttpMethod", method))
            using (LogContext.PushProperty("RequestPath", path))
            using (LogContext.PushProperty("QueryString", query))
            using (LogContext.PushProperty("StatusCode", statusCode))
            using (LogContext.PushProperty("ElapsedMs", elapsed))
            {
                if (ShouldLogHeaders())
                {
                    var headers = SanitiseHeaders(context.Request.Headers);
                    using (LogContext.PushProperty("RequestHeaders", headers, destructureObjects: true))
                    {
                        WriteLog(statusCode, method, path, query, elapsed, correlationId);
                    }
                }
                else
                {
                    WriteLog(statusCode, method, path, query, elapsed, correlationId);
                }
            }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private bool IsEnabled() =>
        configuration.GetValue("Logging:Http:Enabled", defaultValue: true);

    private bool ShouldLogHeaders() =>
        configuration.GetValue("Logging:Http:LogRequestHeaders", defaultValue: true);

    private static Dictionary<string, string> SanitiseHeaders(IHeaderDictionary headers)
    {
        var result = new Dictionary<string, string>(headers.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in headers)
        {
            result[key] = SensitiveHeaders.Contains(key)
                ? MaskedValue
                : FormatHeaderValue(value);
        }
        return result;
    }

    private static string FormatHeaderValue(StringValues value) =>
        value.Count switch
        {
            0 => string.Empty,
            1 => value[0] ?? string.Empty,
            _ => string.Join(", ", value!),
        };

    private static void WriteLog(
        int statusCode, string method, string path,
        string query, long elapsed, string correlationId)
    {
        var level = statusCode switch
        {
            >= 500 => Serilog.Events.LogEventLevel.Error,
            >= 400 => Serilog.Events.LogEventLevel.Warning,
            _      => Serilog.Events.LogEventLevel.Information,
        };

        Log.Write(level,
            "[{CorrelationId}] {HttpMethod} {RequestPath}{QueryString} → {StatusCode} in {ElapsedMs}ms",
            correlationId, method, path, query, statusCode, elapsed);
    }
}
