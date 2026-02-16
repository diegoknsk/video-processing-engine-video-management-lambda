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
        entity.Status.Should().Be("Pending");
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
            Status = "Processing",
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
        video.Status.Should().Be(VideoStatus.Processing);
        video.ProcessingMode.Should().Be(ProcessingMode.FanOut);
        video.ProgressPercent.Should().Be(50);
        video.CreatedAt.Should().Be(DateTime.Parse(entity.CreatedAt));
        video.ClientRequestId.Should().Be("req-123");
    }
}
