using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.UseCases.Posts;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Presentation.Controllers;

/// <summary>
/// Endpoints for managing blog posts and their associated comments.
/// </summary>
[Route("api/posts")]
[Produces("application/json")]
[Tags("Posts")]
public sealed class PostsController : ApiControllerBase
{
    /// <summary>
    /// Lists all blog posts.
    /// </summary>
    /// <remarks>
    /// Returns each post's id, title, creation date and total number of comments,
    /// ordered by creation date descending. Returns an empty array when no posts exist.
    /// </remarks>
    /// <param name="useCase">Resolved by the DI container.</param>
    /// <param name="cancellationToken">Aborts the request if the caller disconnects.</param>
    /// <response code="200">The list of posts.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PostListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromServices] GetAllPostsUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves a specific blog post with its content and all associated comments.
    /// </summary>
    /// <param name="id">The unique identifier of the post.</param>
    /// <param name="useCase">Resolved by the DI container.</param>
    /// <param name="cancellationToken">Aborts the request if the caller disconnects.</param>
    /// <response code="200">The post with its title, content, creation date and comments ordered by creation date ascending.</response>
    /// <response code="404">No post exists for the supplied id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet("{id:guid}", Name = nameof(GetById))]
    [ProducesResponseType(typeof(PostDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] GetPostByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new blog post.
    /// </summary>
    /// <remarks>
    /// Validation rules:
    /// - Title is required and must not exceed 200 characters.
    /// - Content is required and must not exceed 10,000 characters.
    ///
    /// On success the response contains the created post. The Location header points to
    /// the GET endpoint for the new resource.
    /// </remarks>
    /// <param name="request">The post payload (title and content).</param>
    /// <param name="useCase">Resolved by the DI container.</param>
    /// <param name="cancellationToken">Aborts the request if the caller disconnects.</param>
    /// <response code="201">The post was created.</response>
    /// <response code="400">The request payload failed validation.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PostDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePostRequest request,
        [FromServices] CreatePostUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(request, cancellationToken);
        return HandleResult(result, post =>
            CreatedAtRoute(nameof(GetById), new { id = post.Id }, post));
    }
}
