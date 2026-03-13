using FluentAssertions;
using VideoProcessing.VideoManagement.Domain.Entities;
using VideoProcessing.VideoManagement.Domain.Enums;
using VideoProcessing.VideoManagement.Infra.Data.Mappers;
using VideoProcessing.VideoManagement.Infra.Data.Repositories;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Infra.Data.Mappers;

public class VideoMapperTests
{
    [Fact]
    public void ToEntity_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1000, "req-123");
        video.SetExecutionArn("arn:aws:states:...");
        
        // Act
        var entity = VideoMapper.ToEntity(video);

        // Assert
        entity.Pk.Should().Be($"USER#{userId}");
        entity.Sk.Should().Be($"VIDEO#{video.VideoId}");
        entity.VideoId.Should().Be(video.VideoId.ToString());
        entity.Status.Should().Be("UploadPending");
        entity.ClientRequestId.Should().Be("req-123");
        entity.StepExecutionArn.Should().Be("arn:aws:states:...");
    }

    [Fact]
    public void ToDomain_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var videoId = Guid.NewGuid();
        var created = DateTime.UtcNow;
        var entity = new VideoEntity
        {
            Pk = $"USER#{userId}",
            Sk = $"VIDEO#{videoId}",
            UserId = userId.ToString(),
            VideoId = videoId.ToString(),
            Status = "ProcessingImages",
            ProcessingMode = "FanOut",
            ProgressPercent = 50,
            CreatedAt = created.ToString("O"),
            ClientRequestId = "req-123"
        };

        // Act
        var video = VideoMapper.ToDomain(entity);

        // Assert
        video.VideoId.Should().Be(videoId);
        video.UserId.Should().Be(userId);
        video.Status.Should().Be(VideoStatus.ProcessingImages);
        video.ProcessingMode.Should().Be(ProcessingMode.FanOut);
        video.ProgressPercent.Should().Be(50);
        video.CreatedAt.Should().Be(DateTime.Parse(entity.CreatedAt));
        video.ClientRequestId.Should().Be("req-123");
    }

    [Fact]
    public void ToEntity_WithParallelChunksAndZipFields_ShouldMapCorrectly()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1000);
        video.SetParallelChunks(4);
        var entity = new VideoEntity
        {
            Pk = $"USER#{userId}",
            Sk = $"VIDEO#{video.VideoId}",
            UserId = userId.ToString(),
            VideoId = video.VideoId.ToString(),
            Status = "UploadPending",
            ProcessingMode = "SingleLambda",
            ProgressPercent = 0,
            ParallelChunks = 4,
            ZipBucket = "zip-bucket",
            ZipKey = "path/file.zip",
            ZipFileName = "download.zip",
            CreatedAt = DateTime.UtcNow.ToString("O")
        };
        var domain = VideoMapper.ToDomain(entity);

        domain.ParallelChunks.Should().Be(4);
        domain.ZipBucket.Should().Be("zip-bucket");
        domain.ZipKey.Should().Be("path/file.zip");
        domain.ZipFileName.Should().Be("download.zip");
    }

    [Fact]
    public void ToEntity_WhenVideoHasUserEmail_ShouldMapUserEmail()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1000, null, "user@example.com");

        var entity = VideoMapper.ToEntity(video);

        entity.UserEmail.Should().Be("user@example.com");
    }

    [Fact]
    public void ToDomain_WhenEntityHasUserEmail_ShouldMapUserEmail()
    {
        var userId = Guid.NewGuid();
        var videoId = Guid.NewGuid();
        var entity = new VideoEntity
        {
            Pk = $"USER#{userId}",
            Sk = $"VIDEO#{videoId}",
            UserId = userId.ToString(),
            VideoId = videoId.ToString(),
            UserEmail = "owner@example.com",
            OriginalFileName = "f.mp4",
            ContentType = "video/mp4",
            SizeBytes = 1000,
            Status = "UploadPending",
            ProcessingMode = "SingleLambda",
            ProgressPercent = 0,
            CreatedAt = DateTime.UtcNow.ToString("O")
        };

        var video = VideoMapper.ToDomain(entity);

        video.UserEmail.Should().Be("owner@example.com");
    }

    [Fact]
    public void ToDomain_WhenEntityHasNoUserEmail_ShouldHaveNullUserEmail()
    {
        var userId = Guid.NewGuid();
        var videoId = Guid.NewGuid();
        var entity = new VideoEntity
        {
            Pk = $"USER#{userId}",
            Sk = $"VIDEO#{videoId}",
            UserId = userId.ToString(),
            VideoId = videoId.ToString(),
            UserEmail = null,
            OriginalFileName = "f.mp4",
            ContentType = "video/mp4",
            SizeBytes = 1000,
            Status = "UploadPending",
            ProcessingMode = "SingleLambda",
            ProgressPercent = 0,
            CreatedAt = DateTime.UtcNow.ToString("O")
        };

        var video = VideoMapper.ToDomain(entity);

        video.UserEmail.Should().BeNull();
    }
}
