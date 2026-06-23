namespace BlogPlatform.Application.DTOs.Posts;

public sealed record CommentDto(
    Guid Id,
    string Author,
    string Content,
    DateTime CreatedAt);
