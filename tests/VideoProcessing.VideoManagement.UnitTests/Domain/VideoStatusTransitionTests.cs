using FluentAssertions;
using VideoProcessing.VideoManagement.Domain.Entities;
using VideoProcessing.VideoManagement.Domain.Enums;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Domain;

public class VideoStatusTransitionTests
{
    [Fact]
    public void UpdateStatus_ProcessingImagesToGeneratingZip_SetsImagesProcessingCompletedAt()
    {
        var video = new Video(Guid.NewGuid(), "test.mp4", "video/mp4", 1024);
        video.UpdateStatus(VideoStatus.ProcessingImages);
        video.ImagesProcessingCompletedAt.Should().BeNull();
        video.UpdateStatus(VideoStatus.GeneratingZip);
        video.ImagesProcessingCompletedAt.Should().NotBeNull();
        video.ImagesProcessingCompletedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void UpdateStatus_GeneratingZipToCompleted_SetsProcessingCompletedAt()
    {
        var video = new Video(Guid.NewGuid(), "test.mp4", "video/mp4", 1024);
        video.UpdateStatus(VideoStatus.ProcessingImages);
        video.UpdateStatus(VideoStatus.GeneratingZip);
        video.ProcessingCompletedAt.Should().BeNull();
        video.UpdateStatus(VideoStatus.Completed);
        video.ProcessingCompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateStatus_ToFailed_SetsLastFailedAt()
    {
        var video = new Video(Guid.NewGuid(), "test.mp4", "video/mp4", 1024);
        video.UpdateStatus(VideoStatus.ProcessingImages);
        video.LastFailedAt.Should().BeNull();
        video.UpdateStatus(VideoStatus.Failed);
        video.LastFailedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateStatus_ToCancelled_SetsLastCancelledAt()
    {
        var video = new Video(Guid.NewGuid(), "test.mp4", "video/mp4", 1024);
        video.UpdateStatus(VideoStatus.ProcessingImages);
        video.UpdateStatus(VideoStatus.Cancelled);
        video.LastCancelledAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateStatus_UploadPendingToProcessingImages_NoTransitionTimestamps()
    {
        var video = new Video(Guid.NewGuid(), "test.mp4", "video/mp4", 1024);
        video.UpdateStatus(VideoStatus.ProcessingImages);
        video.ImagesProcessingCompletedAt.Should().BeNull();
        video.ProcessingCompletedAt.Should().BeNull();
        video.LastFailedAt.Should().BeNull();
        video.LastCancelledAt.Should().BeNull();
    }

    [Fact]
    public void UpdateStatus_CompletedToProcessingImages_Throws()
    {
        var video = new Video(Guid.NewGuid(), "test.mp4", "video/mp4", 1024);
        video.UpdateStatus(VideoStatus.ProcessingImages);
        video.UpdateStatus(VideoStatus.GeneratingZip);
        video.UpdateStatus(VideoStatus.Completed);
        var act = () => video.UpdateStatus(VideoStatus.ProcessingImages);
        act.Should().Throw<InvalidOperationException>().WithMessage("*Cannot transition*");
    }

    [Fact]
    public void MarkAsFailed_SetsLastFailedAt()
    {
        var video = new Video(Guid.NewGuid(), "test.mp4", "video/mp4", 1024);
        video.MarkAsFailed("erro");
        video.Status.Should().Be(VideoStatus.Failed);
        video.LastFailedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsCompleted_SetsProcessingCompletedAt()
    {
        var video = new Video(Guid.NewGuid(), "test.mp4", "video/mp4", 1024);
        video.UpdateStatus(VideoStatus.ProcessingImages);
        video.UpdateStatus(VideoStatus.GeneratingZip);
        video.MarkAsCompleted();
        video.Status.Should().Be(VideoStatus.Completed);
        video.ProcessingCompletedAt.Should().NotBeNull();
    }
}
