using BlogPlatform.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Presentation.Middleware;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is DomainValidationException validationException)
        {
            await WriteValidationProblemAsync(httpContext, validationException, cancellationToken);
            return true;
        }

        logger.LogError(
            exception,
            "Unhandled exception while processing {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        await WriteUnexpectedProblemAsync(httpContext, cancellationToken);
        return true;
    }

    private static async Task WriteValidationProblemAsync(
        HttpContext httpContext,
        DomainValidationException exception,
        CancellationToken cancellationToken)
    {
        var problem = new ValidationProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed.",
            Detail = string.Join(" ", exception.Notifications),
            Instance = httpContext.Request.Path,
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        problem.Extensions["notifications"] = exception.Notifications;

        httpContext.Response.StatusCode = problem.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
    }

    private static async Task WriteUnexpectedProblemAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Instance = httpContext.Request.Path,
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = problem.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
    }
}
