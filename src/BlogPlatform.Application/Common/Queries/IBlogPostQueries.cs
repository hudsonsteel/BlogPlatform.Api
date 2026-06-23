using BlogPlatform.Application.DTOs.Posts;

namespace BlogPlatform.Application.Common.Queries;

public interface IBlogPostQueries
{
    Task<IReadOnlyList<PostListItemDto>> ListAsync(CancellationToken cancellationToken);
}
