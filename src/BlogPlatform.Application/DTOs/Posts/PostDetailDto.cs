namespace BlogPlatform.Application.DTOs.Posts;

public sealed record PostDetailDto(
    Guid Id,
    string Title,
    string Content,
    DateTime CreatedAt,
    IReadOnlyList<CommentDto> Comments);
