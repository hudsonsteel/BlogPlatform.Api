using BlogPlatform.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Presentation.Controllers;

/// <summary>
/// Base class for API controllers. Provides the <see cref="HandleResult{T}(Result{T})"/> helper
/// that translates a <see cref="Result"/> into the appropriate <see cref="IActionResult"/>
/// (HTTP status + ProblemDetails body for failures).
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Translates a typed <see cref="Result{T}"/> into <see cref="OkObjectResult"/> on success
    /// or a ProblemDetails response that matches the error type.
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result) =>
        result.IsSuccess
            ? Ok(result.Value)
            : ToProblem(result.Error);

    /// <summary>
    /// Translates a typed <see cref="Result{T}"/> into the supplied success handler
    /// (e.g. <see cref="CreatedAtActionResult"/>) on success or a ProblemDetails response on failure.
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result, Func<T, IActionResult> onSuccess) =>
        result.IsSuccess
            ? onSuccess(result.Value)
            : ToProblem(result.Error);

    /// <summary>
    /// Translates a non-generic <see cref="Result"/> into <see cref="NoContentResult"/> on success
    /// or a ProblemDetails response on failure.
    /// </summary>
    protected IActionResult HandleResult(Result result) =>
        result.IsSuccess
            ? NoContent()
            : ToProblem(result.Error);

    private IActionResult ToProblem(Error error) => error.Type switch
    {
        ErrorType.Validation => ValidationProblem(detail: error.Message, title: error.Code),
        ErrorType.NotFound => Problem(detail: error.Message, statusCode: StatusCodes.Status404NotFound, title: error.Code),
        ErrorType.Conflict => Problem(detail: error.Message, statusCode: StatusCodes.Status409Conflict, title: error.Code),
        _ => Problem(detail: error.Message, statusCode: StatusCodes.Status500InternalServerError, title: error.Code),
    };
}
