using FluentAssertions;
using Moq;
using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.UseCases.UpdateVideo;
using VideoProcessing.VideoManagement.Domain.Entities;
using VideoProcessing.VideoManagement.Domain.Enums;
using VideoProcessing.VideoManagement.Domain.Exceptions;
using VideoProcessing.VideoManagement.Application.Ports;

namespace VideoProcessing.VideoManagement.UnitTests.Application.UseCases.UpdateVideo;

public class UpdateVideoUseCaseTests
{
    private readonly Mock<IVideoRepository> _repositoryMock;
    private readonly UpdateVideoUseCase _sut;

    public UpdateVideoUseCaseTests()
    {
        _repositoryMock = new Mock<IVideoRepository>();
        _sut = new UpdateVideoUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateStatus_ShouldReturnVideoResponseModelWithUpdatedStatus()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        var videoId = video.VideoId;
        var patch = new VideoUpdateValues(VideoStatus.Processing, null, null, null, null, null, null, null, null);
        var updatedVideo = Video.FromMerge(video, patch);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Video>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedVideo);

        var input = new UpdateVideoInputModel { UserId = userId, Status = VideoStatus.Processing };

        var result = await _sut.ExecuteAsync(videoId, input, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(VideoStatus.Processing);
        result.VideoId.Should().Be(video.VideoId);
        _repositoryMock.Verify(r => r.GetByIdAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Video>(v => v.Status == VideoStatus.Processing), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateProgressPercent_ShouldReturnVideoResponseModelWithUpdatedProgress()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        var videoId = video.VideoId;
        var patch = new VideoUpdateValues(null, 50, null, null, null, null, null, null, null);
        var updatedVideo = Video.FromMerge(video, patch);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Video>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedVideo);

        var input = new UpdateVideoInputModel { UserId = userId, ProgressPercent = 50 };

        var result = await _sut.ExecuteAsync(videoId, input, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ProgressPercent.Should().Be(50);
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Video>(v => v.ProgressPercent == 50), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleFields_ShouldApplyAllAndPreserveUnsetFields()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        var videoId = video.VideoId;
        var patch = new VideoUpdateValues(VideoStatus.Processing, 75, "Erro de rede", "NETWORK_ERROR", null, null, null, null, null);
        var updatedVideo = Video.FromMerge(video, patch);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Video>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedVideo);

        var input = new UpdateVideoInputModel
        {
            UserId = userId,
            Status = VideoStatus.Processing,
            ProgressPercent = 75,
            ErrorMessage = "Erro de rede",
            ErrorCode = "NETWORK_ERROR"
        };

        var result = await _sut.ExecuteAsync(videoId, input, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(VideoStatus.Processing);
        result.ProgressPercent.Should().Be(75);
        result.ErrorMessage.Should().Be("Erro de rede");
        result.ErrorCode.Should().Be("NETWORK_ERROR");
        result.OriginalFileName.Should().Be("test.mp4");
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Video>(v =>
            v.Status == VideoStatus.Processing && v.ProgressPercent == 75 && v.ErrorMessage == "Erro de rede" && v.ErrorCode == "NETWORK_ERROR"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_VideoNotFound_ShouldReturnNull()
    {
        var userId = Guid.NewGuid();
        var videoId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Video?)null);

        var input = new UpdateVideoInputModel { UserId = userId, Status = VideoStatus.Processing };

        var result = await _sut.ExecuteAsync(videoId, input, CancellationToken.None);

        result.Should().BeNull();
        _repositoryMock.Verify(r => r.GetByIdAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Video>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateAsyncThrowsVideoUpdateConflictException_ShouldPropagateException()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        var videoId = video.VideoId;

        _repositoryMock
            .Setup(r => r.GetByIdAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Video>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new VideoUpdateConflictException("Update failed: ownership mismatch."));

        var input = new UpdateVideoInputModel { UserId = userId, ProgressPercent = 50 };

        var act = () => _sut.ExecuteAsync(videoId, input, CancellationToken.None);

        await act.Should().ThrowAsync<VideoUpdateConflictException>()
            .WithMessage("Update failed: ownership mismatch.");
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Video>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
