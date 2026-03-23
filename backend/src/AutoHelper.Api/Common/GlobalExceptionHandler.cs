using AutoHelper.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AutoHelper.Api.Common;

/// <summary>
/// Central exception → HTTP response mapping.
/// Domain/validation exceptions become 4xx; everything else becomes 500.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, errors) = exception switch
        {
            DomainException e => (
                StatusCodes.Status400BadRequest,
                e.Message,
                (IDictionary<string, string[]>?)null),

            NotFoundException e => (
                StatusCodes.Status404NotFound,
                e.Message,
                (IDictionary<string, string[]>?)null),

            ValidationException e => (
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                (IDictionary<string, string[]>?)e.Errors
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(f => f.ErrorMessage).ToArray())),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized.",
                (IDictionary<string, string[]>?)null),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                (IDictionary<string, string[]>?)null)
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");

        var problemDetails = new ValidationProblemDetails
        {
            Status = statusCode,
            Title = title,
            Instance = context.Request.Path
        };

        if (errors is not null)
            problemDetails.Errors = errors;

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
