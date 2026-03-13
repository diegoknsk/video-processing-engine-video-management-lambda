using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VideoProcessing.VideoManagement.Application.Configuration;
using VideoProcessing.VideoManagement.Application.Models;
using VideoProcessing.VideoManagement.Application.Ports;
using VideoProcessing.VideoManagement.Application.Services;
using VideoProcessing.VideoManagement.Application.UseCases.GetVideoById;
using VideoProcessing.VideoManagement.Domain.Entities;
using VideoProcessing.VideoManagement.Domain.Enums;

namespace VideoProcessing.VideoManagement.UnitTests.Application.UseCases.GetVideoById;

public class GetVideoByIdUseCaseTests
{
    private readonly Mock<IVideoRepository> _repositoryMock;
    private readonly Mock<IVideoChunkRepository> _chunkRepositoryMock;
    private readonly Mock<IChunkProgressCalculator> _chunkProgressCalculatorMock;
    private readonly Mock<IS3PresignedUrlService> _s3PresignedUrlMock;
    private readonly GetVideoByIdUseCase _sut;

    public GetVideoByIdUseCaseTests()
    {
        _repositoryMock = new Mock<IVideoRepository>();
        _chunkRepositoryMock = new Mock<IVideoChunkRepository>();
        var emptySummary = new ChunkStatusSummary(0, 0, 0, 0, 0, null);
        _chunkRepositoryMock.Setup(c => c.GetStatusSummaryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(emptySummary);
        _chunkRepositoryMock.Setup(c => c.GetChunksAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<VideoChunk>());

        _chunkProgressCalculatorMock = new Mock<IChunkProgressCalculator>();
        _chunkProgressCalculatorMock
            .Setup(x => x.Calculate(It.IsAny<VideoStatus>(), It.IsAny<ChunkStatusSummary?>()))
            .Returns((VideoStatus s, ChunkStatusSummary? sum) => new ChunkProgressResult(
                sum is { Total: > 0 } ? (int)Math.Floor((sum.Completed * 100.0) / sum.Total) : -1,
                s.ToString(),
                sum is { Total: > 0 }));

        _s3PresignedUrlMock = new Mock<IS3PresignedUrlService>();
        var s3Options = Options.Create(new S3Options { PresignedUrlTtlMinutes = 15 });
        var logger = new Mock<ILogger<GetVideoByIdUseCase>>();
        _sut = new GetVideoByIdUseCase(
            _repositoryMock.Object,
            _chunkRepositoryMock.Object,
            _chunkProgressCalculatorMock.Object,
            _s3PresignedUrlMock.Object,
            s3Options,
            logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_VideoFound_ShouldReturnVideoResponseModel()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(userId.ToString(), video.VideoId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        // Act
        var result = await _sut.ExecuteAsync(userId.ToString(), video.VideoId.ToString(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.VideoId.Should().Be(video.VideoId);
        result.UserId.Should().Be(video.UserId);
        result.OriginalFileName.Should().Be(video.OriginalFileName);
        result.ContentType.Should().Be(video.ContentType);
        result.SizeBytes.Should().Be(video.SizeBytes);
        result.Status.Should().Be(video.Status);

        _repositoryMock.Verify(r => r.GetByIdAsync(userId.ToString(), video.VideoId.ToString(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_VideoNotFound_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var videoId = Guid.NewGuid().ToString();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(userId, videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Video?)null);

        // Act
        var result = await _sut.ExecuteAsync(userId, videoId, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _repositoryMock.Verify(r => r.GetByIdAsync(userId, videoId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenParallelChunks4AndCount2_ShouldReturnProgressPercent50()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        video.SetParallelChunks(4);

        var summary = new ChunkStatusSummary(4, 2, 2, 0, 0, null);
        _repositoryMock.Setup(r => r.GetByIdAsync(userId.ToString(), video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(video);
        _chunkRepositoryMock.Setup(c => c.GetStatusSummaryAsync(video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(summary);
        _chunkRepositoryMock.Setup(c => c.GetChunksAsync(video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<VideoChunk>());

        var result = await _sut.ExecuteAsync(userId.ToString(), video.VideoId.ToString(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ProgressPercent.Should().Be(50);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStatusCompleted_ShouldReturnProgressPercent100()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        video.SetParallelChunks(4);
        video.MarkAsCompleted();

        _repositoryMock.Setup(r => r.GetByIdAsync(userId.ToString(), video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(video);

        var result = await _sut.ExecuteAsync(userId.ToString(), video.VideoId.ToString(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ProgressPercent.Should().Be(100);
    }

    [Fact]
    public async Task ExecuteAsync_WhenZipKeyAndZipBucketPresent_ShouldReturnZipUrl()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        var data = new VideoRehydrationData(
            video.VideoId, video.UserId, video.UserEmail, video.OriginalFileName, video.ContentType, video.SizeBytes,
            video.DurationSec, video.FrameIntervalSec, video.Status, video.ProcessingMode, video.ProgressPercent,
            video.S3BucketVideo, video.S3KeyVideo, video.S3BucketZip, video.S3KeyZip,
            "my-bucket", "path/result.zip", "resultado.zip",
            video.S3BucketFrames, video.FramesPrefix, video.StepExecutionArn, video.ErrorMessage, video.ErrorCode,
            video.ClientRequestId, video.ChunkCount, video.ChunkDurationSec, video.UploadIssuedAt, video.UploadUrlExpiresAt,
            video.FramesProcessed, video.FinalizedAt, video.ParallelChunks, video.ProcessingSummary,
            video.ProcessingStartedAt, video.ImagesProcessingCompletedAt, video.ProcessingCompletedAt,
            video.LastFailedAt, video.LastCancelledAt, video.CreatedAt, video.UpdatedAt, video.Version);
        var videoWithZip = Video.FromPersistence(data);

        _repositoryMock.Setup(r => r.GetByIdAsync(userId.ToString(), video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(videoWithZip);
        _s3PresignedUrlMock.Setup(s => s.GenerateGetPresignedUrl("my-bucket", "path/result.zip", It.IsAny<TimeSpan>())).Returns("https://s3.amazonaws.com/my-bucket/path/result.zip?signature=...");

        var result = await _sut.ExecuteAsync(userId.ToString(), video.VideoId.ToString(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ZipUrl.Should().Be("https://s3.amazonaws.com/my-bucket/path/result.zip?signature=...");
        result.ZipFileName.Should().Be("resultado.zip");
    }

    [Fact]
    public async Task ExecuteAsync_WhenZipKeyAbsent_ShouldReturnZipUrlNull()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);

        _repositoryMock.Setup(r => r.GetByIdAsync(userId.ToString(), video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(video);

        var result = await _sut.ExecuteAsync(userId.ToString(), video.VideoId.ToString(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ZipUrl.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenPresignedUrlThrows_ShouldReturnZipUrlNullWithoutPropagating()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        var data = new VideoRehydrationData(
            video.VideoId, video.UserId, video.UserEmail, video.OriginalFileName, video.ContentType, video.SizeBytes,
            video.DurationSec, video.FrameIntervalSec, video.Status, video.ProcessingMode, video.ProgressPercent,
            video.S3BucketVideo, video.S3KeyVideo, video.S3BucketZip, video.S3KeyZip,
            "bucket", "key.zip", "file.zip",
            video.S3BucketFrames, video.FramesPrefix, video.StepExecutionArn, video.ErrorMessage, video.ErrorCode,
            video.ClientRequestId, video.ChunkCount, video.ChunkDurationSec, video.UploadIssuedAt, video.UploadUrlExpiresAt,
            video.FramesProcessed, video.FinalizedAt, video.ParallelChunks, video.ProcessingSummary,
            video.ProcessingStartedAt, video.ImagesProcessingCompletedAt, video.ProcessingCompletedAt,
            video.LastFailedAt, video.LastCancelledAt, video.CreatedAt, video.UpdatedAt, video.Version);
        var videoWithZip = Video.FromPersistence(data);

        _repositoryMock.Setup(r => r.GetByIdAsync(userId.ToString(), video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(videoWithZip);
        _s3PresignedUrlMock.Setup(s => s.GenerateGetPresignedUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>())).Throws(new InvalidOperationException("S3 error"));

        var result = await _sut.ExecuteAsync(userId.ToString(), video.VideoId.ToString(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ZipUrl.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoredProgressPercent50AndChunksEmpty_ShouldReturnProgressPercent50()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        video.UpdateStatus(VideoStatus.ProcessingImages);
        video.SetProgress(50);
        video.SetParallelChunks(4);

        _repositoryMock.Setup(r => r.GetByIdAsync(userId.ToString(), video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(video);

        var result = await _sut.ExecuteAsync(userId.ToString(), video.VideoId.ToString(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ProgressPercent.Should().Be(50);
    }

    [Fact]
    public async Task ExecuteAsync_WhenComputedFromChunksExceedsStoredProgress_ShouldReturnComputedValue()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        video.UpdateStatus(VideoStatus.ProcessingImages);
        video.SetProgress(30);
        video.SetParallelChunks(4);

        var summary = new ChunkStatusSummary(4, 2, 2, 0, 0, null);
        _repositoryMock.Setup(r => r.GetByIdAsync(userId.ToString(), video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(video);
        _chunkRepositoryMock.Setup(c => c.GetStatusSummaryAsync(video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(summary);
        _chunkRepositoryMock.Setup(c => c.GetChunksAsync(video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<VideoChunk>());

        var result = await _sut.ExecuteAsync(userId.ToString(), video.VideoId.ToString(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ProgressPercent.Should().Be(50);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStatusCompletedIgnoresStoredProgressAndChunks_ShouldReturn100()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        video.SetParallelChunks(4);
        video.MarkAsCompleted();

        _repositoryMock.Setup(r => r.GetByIdAsync(userId.ToString(), video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(video);

        var result = await _sut.ExecuteAsync(userId.ToString(), video.VideoId.ToString(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ProgressPercent.Should().Be(100);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoredProgressZeroAndChunksEmptyAndParallelChunksNull_ShouldReturnProgressPercent0()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        video.UpdateStatus(VideoStatus.ProcessingImages);

        _repositoryMock.Setup(r => r.GetByIdAsync(userId.ToString(), video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(video);

        var result = await _sut.ExecuteAsync(userId.ToString(), video.VideoId.ToString(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ProgressPercent.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHasChunks_ShouldReturnChunksSummaryAndChunksList()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        video.UpdateStatus(VideoStatus.ProcessingImages);
        video.SetParallelChunks(3);

        var summary = new ChunkStatusSummary(3, 2, 1, 0, 0, null);
        var chunks = new List<VideoChunk>
        {
            new("chunk-0", video.VideoId.ToString(), "completed", 0, 15, 1, null, null, DateTime.UtcNow, DateTime.UtcNow),
            new("chunk-1", video.VideoId.ToString(), "completed", 15, 30, 1, null, null, DateTime.UtcNow, DateTime.UtcNow),
            new("chunk-2", video.VideoId.ToString(), "processing", 30, 45, 1, null, null, null, DateTime.UtcNow)
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(userId.ToString(), video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(video);
        _chunkRepositoryMock.Setup(c => c.GetStatusSummaryAsync(video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(summary);
        _chunkRepositoryMock.Setup(c => c.GetChunksAsync(video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(chunks);

        var result = await _sut.ExecuteAsync(userId.ToString(), video.VideoId.ToString(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ChunksSummary.Should().NotBeNull();
        result.ChunksSummary!.Total.Should().Be(3);
        result.ChunksSummary.Completed.Should().Be(2);
        result.ChunksSummary.Processing.Should().Be(1);
        result.Chunks.Should().HaveCount(3);
        result.Chunks!.Select(x => x.ChunkId).Should().ContainInOrder("chunk-0", "chunk-1", "chunk-2");
        result.CurrentStage.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoChunks_ShouldReturnNullChunksSummaryAndNullChunks()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);

        _repositoryMock.Setup(r => r.GetByIdAsync(userId.ToString(), video.VideoId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync(video);

        var result = await _sut.ExecuteAsync(userId.ToString(), video.VideoId.ToString(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ChunksSummary.Should().BeNull();
        result.Chunks.Should().BeNull();
    }
}
