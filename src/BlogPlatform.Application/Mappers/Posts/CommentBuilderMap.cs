using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Application.Mappers.Posts;

public static class CommentBuilderMap
{
    public static Comment From(AddCommentRequest request, Guid blogPostId) =>
        new(blogPostId, request.Author, request.Content);

    public static CommentDto ToDto(Comment comment) =>
        new(comment.Id, comment.Author, comment.Content, comment.CreatedAt);
}
