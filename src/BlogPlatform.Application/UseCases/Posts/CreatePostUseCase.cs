using BlogPlatform.Application.Common.Models;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.Mappers.Posts;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Domain.Repositories;
using FluentValidation;

namespace BlogPlatform.Application.UseCases.Posts;

public sealed class CreatePostUseCase(
    IBlogPostRepository repository,
    IUnitOfWork unitOfWork,
    IValidator<BlogPost> validator)
{
    public async Task<Result<PostDetailDto>> Handle(
        CreatePostRequest request,
        CancellationToken cancellationToken)
    {
        var post = BlogPostBuilderMap.From(request);
        await post.ThrowIfInvalidAsync(validator, cancellationToken);

        await repository.AddAsync(post, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(BlogPostBuilderMap.ToDetailDto(post));
    }
}
