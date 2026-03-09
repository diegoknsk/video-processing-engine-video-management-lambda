using FluentAssertions;
using Microsoft.Extensions.Logging;
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
    private readonly Mock<ILogger<UpdateVideoUseCase>> _loggerMock;
    private readonly UpdateVideoUseCase _sut;

    public UpdateVideoUseCaseTests()
    {
        _repositoryMock = new Mock<IVideoRepository>();
        _loggerMock = new Mock<ILogger<UpdateVideoUseCase>>();
        _sut = new UpdateVideoUseCase(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateStatus_ShouldReturnVideoResponseModelWithUpdatedStatus()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        var videoId = video.VideoId;
        var patch = new VideoUpdateValues(VideoStatus.ProcessingImages, null, null, null, null, null, null, null, null, null, null, null);
        var updatedVideo = Video.FromMerge(video, patch);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Video>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedVideo);

        var input = new UpdateVideoInputModel { UserId = userId, Status = VideoStatus.ProcessingImages };

        var result = await _sut.ExecuteAsync(videoId, input, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(VideoStatus.ProcessingImages);
        result.VideoId.Should().Be(video.VideoId);
        _repositoryMock.Verify(r => r.GetByIdAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Video>(v => v.Status == VideoStatus.ProcessingImages), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateProgressPercent_ShouldReturnVideoResponseModelWithUpdatedProgress()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        var videoId = video.VideoId;
        var patch = new VideoUpdateValues(null, 50, null, null, null, null, null, null, null, null, null, null);
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
        var patch = new VideoUpdateValues(VideoStatus.ProcessingImages, 75, "Erro de rede", "NETWORK_ERROR", null, null, null, null, null, null, null, null);
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
            Status = VideoStatus.ProcessingImages,
            ProgressPercent = 75,
            ErrorMessage = "Erro de rede",
            ErrorCode = "NETWORK_ERROR"
        };

        var result = await _sut.ExecuteAsync(videoId, input, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(VideoStatus.ProcessingImages);
        result.ProgressPercent.Should().Be(75);
        result.ErrorMessage.Should().Be("Erro de rede");
        result.ErrorCode.Should().Be("NETWORK_ERROR");
        result.OriginalFileName.Should().Be("test.mp4");
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Video>(v =>
            v.Status == VideoStatus.ProcessingImages && v.ProgressPercent == 75 && v.ErrorMessage == "Erro de rede" && v.ErrorCode == "NETWORK_ERROR"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_VideoNotFound_ShouldReturnNull()
    {
        var userId = Guid.NewGuid();
        var videoId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Video?)null);

        var input = new UpdateVideoInputModel { UserId = userId, Status = VideoStatus.ProcessingImages };

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

    [Fact]
    public async Task ExecuteAsync_WhenStatusChanges_ShouldLogStructuredMessage()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        var videoId = video.VideoId;
        var merged = Video.FromMerge(video, new VideoUpdateValues(VideoStatus.GeneratingZip, null, null, null, null, null, null, null, null, null, null, null));
        merged.UpdateStatus(VideoStatus.GeneratingZip);

        _repositoryMock.Setup(r => r.GetByIdAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(video);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Video>(), It.IsAny<CancellationToken>())).ReturnsAsync(merged);

        await _sut.ExecuteAsync(videoId, new UpdateVideoInputModel { UserId = userId, Status = VideoStatus.GeneratingZip }, CancellationToken.None);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Video status changed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStatusUnchanged_ShouldNotLogStatusChange()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        video.UpdateStatus(VideoStatus.ProcessingImages);
        var videoId = video.VideoId;

        _repositoryMock.Setup(r => r.GetByIdAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(video);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Video>(), It.IsAny<CancellationToken>())).ReturnsAsync((Video v, CancellationToken _) => v);

        await _sut.ExecuteAsync(videoId, new UpdateVideoInputModel { UserId = userId, ProgressPercent = 50 }, CancellationToken.None);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Video status changed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithMaxParallelChunks_ShouldPersistValue()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        var videoId = video.VideoId;
        Video? captured = null;
        _repositoryMock.Setup(r => r.GetByIdAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(video);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Video>(), It.IsAny<CancellationToken>()))
            .Callback<Video, CancellationToken>((v, _) => captured = v)
            .ReturnsAsync((Video v, CancellationToken _) => v);

        await _sut.ExecuteAsync(videoId, new UpdateVideoInputModel { UserId = userId, MaxParallelChunks = 10 }, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.MaxParallelChunks.Should().Be(10);
    }

    [Fact]
    public async Task ExecuteAsync_WithProcessingSummary_ShouldMergeChunk()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        var videoId = video.VideoId;
        Video? captured = null;
        _repositoryMock.Setup(r => r.GetByIdAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(video);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Video>(), It.IsAny<CancellationToken>()))
            .Callback<Video, CancellationToken>((v, _) => captured = v)
            .ReturnsAsync((Video v, CancellationToken _) => v);

        var input = new UpdateVideoInputModel
        {
            UserId = userId,
            ProcessingSummary = new ProcessingSummaryInputModel
            {
                Chunks = new Dictionary<string, ChunkInfoInputModel>
                {
                    ["chunk-1"] = new ChunkInfoInputModel { ChunkId = "chunk-1", StartSec = 0, EndSec = 10, IntervalSec = 1, ManifestPrefix = "m/", FramesPrefix = "f/" }
                }
            }
        };

        await _sut.ExecuteAsync(videoId, input, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.ProcessingSummary.Should().NotBeNull();
        captured.ProcessingSummary!.Chunks.Should().ContainKey("chunk-1");
        captured.ProcessingSummary.Chunks["chunk-1"].StartSec.Should().Be(0);
        captured.ProcessingSummary.Chunks["chunk-1"].EndSec.Should().Be(10);
    }
}
