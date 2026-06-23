namespace BlogPlatform.Application.DTOs.Posts;

public sealed record PostListItemDto(
    Guid Id,
    string Title,
    DateTime CreatedAt,
    int CommentCount);
