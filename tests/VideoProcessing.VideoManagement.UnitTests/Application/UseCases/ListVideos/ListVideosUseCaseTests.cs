using FluentAssertions;
using Moq;
using VideoProcessing.VideoManagement.Application.Ports;
using VideoProcessing.VideoManagement.Application.UseCases.ListVideos;
using VideoProcessing.VideoManagement.Domain.Entities;

namespace VideoProcessing.VideoManagement.UnitTests.Application.UseCases.ListVideos;

public class ListVideosUseCaseTests
{
    private readonly Mock<IVideoRepository> _repositoryMock;
    private readonly ListVideosUseCase _sut;

    public ListVideosUseCaseTests()
    {
        _repositoryMock = new Mock<IVideoRepository>();
        _sut = new ListVideosUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithNextToken_ShouldReturnVideosAndNextToken()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var video = new Video(Guid.Parse(userId), "test.mp4", "video/mp4", 1024);
        var nextToken = "token123";

        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, 50, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Video> { video }, nextToken));

        // Act
        var result = await _sut.ExecuteAsync(userId, 50, null, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Videos.Should().HaveCount(1);
        result.Videos[0].VideoId.Should().Be(video.VideoId);
        result.NextToken.Should().Be(nextToken);

        _repositoryMock.Verify(r => r.GetByUserIdAsync(userId, 50, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutNextToken_ShouldReturnVideosWithNullNextToken()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var video = new Video(Guid.Parse(userId), "test.mp4", "video/mp4", 1024);

        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, 50, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Video> { video }, (string?)null));

        // Act
        var result = await _sut.ExecuteAsync(userId, 50, null, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Videos.Should().HaveCount(1);
        result.NextToken.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_EmptyList_ShouldReturnEmptyVideos()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, 50, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Video>(), (string?)null));

        // Act
        var result = await _sut.ExecuteAsync(userId, 50, null, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Videos.Should().BeEmpty();
        result.NextToken.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_LimitGreaterThan100_ShouldClampTo100()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, 100, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Video>(), (string?)null));

        // Act
        await _sut.ExecuteAsync(userId, 150, null, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.GetByUserIdAsync(userId, 100, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNextToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var nextToken = "base64token";

        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, 25, nextToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Video>(), (string?)null));

        // Act
        await _sut.ExecuteAsync(userId, 25, nextToken, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.GetByUserIdAsync(userId, 25, nextToken, It.IsAny<CancellationToken>()), Times.Once);
    }
}
