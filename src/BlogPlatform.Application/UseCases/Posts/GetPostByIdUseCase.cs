using BlogPlatform.Application.Common.Models;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.Mappers.Posts;
using BlogPlatform.Domain.Repositories;

namespace BlogPlatform.Application.UseCases.Posts;

public sealed class GetPostByIdUseCase(IBlogPostRepository repository)
{
    public async Task<Result<PostDetailDto>> Handle(Guid id, CancellationToken cancellationToken)
    {
        var post = await repository.GetByIdAsync(id, cancellationToken);
        if (post is null)
            return Result.Failure<PostDetailDto>(
                Error.NotFound($"Post with id '{id}' was not found."));

        return Result.Success(BlogPostBuilderMap.ToDetailDto(post));
    }
}
