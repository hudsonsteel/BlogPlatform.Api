using BlogPlatform.Application.Common.Models;
using BlogPlatform.Application.UseCases.Posts;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Domain.Repositories;
using BlogPlatform.Tests.Unit.TestSupport;
using FluentAssertions;
using Moq;

namespace BlogPlatform.Tests.Unit.Application.UseCases.Posts;

public sealed class GetPostByIdUseCaseTests
{
    private readonly Mock<IBlogPostRepository> _repository = new();

    [Fact]
    public async Task Handle_WhenPostExists_ReturnsDetailDto()
    {
        // Given
        var postId = Guid.NewGuid();
        var post = new BlogPost("title", "content").WithId(postId);
        _repository
            .Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);
        var useCase = new GetPostByIdUseCase(_repository.Object);

        // When
        var result = await useCase.Handle(postId, CancellationToken.None);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(postId);
        result.Value.Title.Should().Be("title");
        result.Value.Content.Should().Be("content");
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ReturnsNotFoundFailure()
    {
        // Given
        var postId = Guid.NewGuid();
        _repository
            .Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BlogPost?)null);
        var useCase = new GetPostByIdUseCase(_repository.Object);

        // When
        var result = await useCase.Handle(postId, CancellationToken.None);

        // Then
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Message.Should().Contain(postId.ToString());
    }
}
