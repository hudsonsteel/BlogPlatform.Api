using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Application.Mappers.Posts;

public static class BlogPostBuilderMap
{
    public static BlogPost From(CreatePostRequest request) =>
        new(request.Title, request.Content);

    public static PostListItemDto ToListItemDto(BlogPost post) =>
        new(post.Id, post.Title, post.CreatedAt, post.Comments.Count);

    public static PostDetailDto ToDetailDto(BlogPost post)
    {
        var comments = post.Comments
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto(c.Id, c.Author, c.Content, c.CreatedAt))
            .ToList();

        return new PostDetailDto(
            post.Id,
            post.Title,
            post.Content,
            post.CreatedAt,
            comments);
    }
}
