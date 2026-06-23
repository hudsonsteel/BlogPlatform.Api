using BlogPlatform.Application.Common.Models;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.UseCases.Posts;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace BlogPlatform.Tests.Unit.Application.UseCases.Posts;

public sealed class CreatePostUseCaseTests
{
    private readonly Mock<IBlogPostRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<BlogPost>> _validator = new();

    [Fact]
    public async Task Handle_WhenValid_PersistsThePostAndReturnsSuccess()
    {
        // Given
        var request = new CreatePostRequest("A new post", "Some content.");
        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<BlogPost>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        var useCase = new CreatePostUseCase(_repository.Object, _unitOfWork.Object, _validator.Object);

        // When
        var result = await useCase.Handle(request, CancellationToken.None);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("A new post");
        result.Value.Content.Should().Be("Some content.");
        result.Value.Comments.Should().BeEmpty();

        _repository.Verify(r => r.AddAsync(It.IsAny<BlogPost>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidatorFails_ThrowsDomainValidationExceptionAndDoesNotPersist()
    {
        // Given
        var request = new CreatePostRequest("", "");
        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<BlogPost>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure("Title", "Title is required."),
                new ValidationFailure("Content", "Content is required."),
            ]));
        var useCase = new CreatePostUseCase(_repository.Object, _unitOfWork.Object, _validator.Object);

        // When
        var act = () => useCase.Handle(request, CancellationToken.None);

        // Then
        (await act.Should().ThrowAsync<DomainValidationException>())
            .Which.Notifications.Should().BeEquivalentTo(["Title is required.", "Content is required."]);

        _repository.Verify(r => r.AddAsync(It.IsAny<BlogPost>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
