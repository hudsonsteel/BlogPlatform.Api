using BlogPlatform.Application.Common.Models;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.UseCases.Posts;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Domain.Repositories;
using BlogPlatform.Tests.Unit.TestSupport;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace BlogPlatform.Tests.Unit.Application.UseCases.Posts;

public sealed class AddCommentToPostUseCaseTests
{
    private readonly Mock<IBlogPostRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<Comment>> _validator = new();

    [Fact]
    public async Task Handle_WhenPostExistsAndCommentIsValid_AttachesCommentAndSaves()
    {
        // Given
        var postId = Guid.NewGuid();
        var post = new BlogPost("title", "content").WithId(postId);
        _repository
            .Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);
        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        var useCase = new AddCommentToPostUseCase(_repository.Object, _unitOfWork.Object, _validator.Object);
        var request = new AddCommentRequest("Alice", "Great post!");

        // When
        var result = await useCase.Handle(postId, request, CancellationToken.None);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Author.Should().Be("Alice");
        result.Value.Content.Should().Be("Great post!");
        post.Comments.Should().ContainSingle().Which.Author.Should().Be("Alice");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ReturnsNotFoundAndDoesNotSave()
    {
        // Given
        var postId = Guid.NewGuid();
        _repository
            .Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BlogPost?)null);
        var useCase = new AddCommentToPostUseCase(_repository.Object, _unitOfWork.Object, _validator.Object);
        var request = new AddCommentRequest("Alice", "Great post!");

        // When
        var result = await useCase.Handle(postId, request, CancellationToken.None);

        // Then
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        _validator.Verify(v => v.ValidateAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValidatorFails_ThrowsDomainValidationExceptionAndDoesNotSave()
    {
        // Given
        var postId = Guid.NewGuid();
        var post = new BlogPost("title", "content").WithId(postId);
        _repository
            .Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);
        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure("Author", "Author is required."),
            ]));
        var useCase = new AddCommentToPostUseCase(_repository.Object, _unitOfWork.Object, _validator.Object);
        var request = new AddCommentRequest("", "Great post!");

        // When
        var act = () => useCase.Handle(postId, request, CancellationToken.None);

        // Then
        (await act.Should().ThrowAsync<DomainValidationException>())
            .Which.Notifications.Should().Contain("Author is required.");

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
