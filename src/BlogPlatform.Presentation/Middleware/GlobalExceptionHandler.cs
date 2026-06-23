using BlogPlatform.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Presentation.Middleware;

internal sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is DomainValidationException validationException)
            return await WriteAsync(
                httpContext,
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                string.Join(" ", validationException.Notifications));

        logger.LogError(
            exception,
            "Unhandled exception while processing {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        return await WriteAsync(
            httpContext,
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred.",
            detail: null);
    }

    private async Task<bool> WriteAsync(
        HttpContext httpContext,
        int statusCode,
        string title,
        string? detail)
    {
        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
            },
        });
    }
}
