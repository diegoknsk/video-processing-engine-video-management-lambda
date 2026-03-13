using FluentAssertions;
using VideoProcessing.VideoManagement.Application.Models.Mappers;
using VideoProcessing.VideoManagement.Domain.Entities;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Application.Models.Mappers;

public class VideoResponseModelMapperTests
{
    [Fact]
    public void ToResponseModel_WhenVideoHasUserEmail_ShouldMapUserEmail()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024, null, "user@example.com");

        var result = VideoResponseModelMapper.ToResponseModel(video);

        result.Should().NotBeNull();
        result.UserEmail.Should().Be("user@example.com");
    }

    [Fact]
    public void ToResponseModel_WhenVideoHasNullUserEmail_ShouldMapNullUserEmail()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);

        var result = VideoResponseModelMapper.ToResponseModel(video);

        result.Should().NotBeNull();
        result.UserEmail.Should().BeNull();
    }
}
