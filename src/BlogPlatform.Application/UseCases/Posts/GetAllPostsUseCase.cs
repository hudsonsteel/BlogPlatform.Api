using BlogPlatform.Application.Common.Models;
using BlogPlatform.Application.Common.Queries;
using BlogPlatform.Application.DTOs.Posts;

namespace BlogPlatform.Application.UseCases.Posts;

public sealed class GetAllPostsUseCase(IBlogPostQueries queries)
{
    public async Task<Result<IReadOnlyList<PostListItemDto>>> Handle(CancellationToken cancellationToken)
    {
        var posts = await queries.ListAsync(cancellationToken);
        return Result.Success(posts);
    }
}
