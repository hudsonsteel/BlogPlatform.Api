using BlogPlatform.Application.Common.Models;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.Mappers.Posts;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Domain.Repositories;
using FluentValidation;

namespace BlogPlatform.Application.UseCases.Posts;

public sealed class AddCommentToPostUseCase(
    IBlogPostRepository repository,
    IUnitOfWork unitOfWork,
    IValidator<Comment> validator)
{
    public async Task<Result<CommentDto>> Handle(
        Guid postId,
        AddCommentRequest request,
        CancellationToken cancellationToken)
    {
        var post = await repository.GetByIdAsync(postId, cancellationToken);
        if (post is null)
            return Result.Failure<CommentDto>(Error.NotFound($"Post with id '{postId}' was not found."));

        var comment = CommentBuilderMap.From(request, post.Id);
        await comment.ThrowIfInvalidAsync(validator, cancellationToken);

        post.AddComment(comment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(CommentBuilderMap.ToDto(comment));
    }
}
