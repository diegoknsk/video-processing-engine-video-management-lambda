using FluentAssertions;
using VideoProcessing.VideoManagement.Domain.Entities;
using VideoProcessing.VideoManagement.Domain.Enums;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Domain;

/// <summary>
/// Testes unitários para a entidade Video (métodos de mutação, validação de construtor e FromMerge).
/// </summary>
public class VideoEntityTests
{
    // ─────────────── Constructor validations ───────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenOriginalFileNameEmpty_ThrowsArgumentException(string fileName)
    {
        var act = () => new Video(Guid.NewGuid(), fileName, "video/mp4", 1024);
        act.Should().Throw<ArgumentException>().WithParameterName("originalFileName");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenContentTypeEmpty_ThrowsArgumentException(string contentType)
    {
        var act = () => new Video(Guid.NewGuid(), "video.mp4", contentType, 1024);
        act.Should().Throw<ArgumentException>().WithParameterName("contentType");
    }

    [Fact]
    public void Constructor_WhenSizeBytesNegative_ThrowsArgumentOutOfRangeException()
    {
        var act = () => new Video(Guid.NewGuid(), "video.mp4", "video/mp4", -1);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("sizeBytes");
    }

    [Fact]
    public void Constructor_WhenValid_SetsStatusToUploadPending()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        video.Status.Should().Be(VideoStatus.UploadPending);
        video.ProgressPercent.Should().Be(0);
    }

    // ─────────────── SetProgress ───────────────

    [Fact]
    public void SetProgress_WhenBelowCurrentProgress_ThrowsInvalidOperationException()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        video.SetProgress(50);

        var act = () => video.SetProgress(30);
        act.Should().Throw<InvalidOperationException>().WithMessage("*cannot regress*");
    }

    [Fact]
    public void SetProgress_WhenAbove100_ThrowsArgumentOutOfRangeException()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        var act = () => video.SetProgress(101);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("percent");
    }

    [Fact]
    public void SetProgress_WhenNegative_ThrowsArgumentOutOfRangeException()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        var act = () => video.SetProgress(-1);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("percent");
    }

    [Fact]
    public void SetProgress_WhenValid_UpdatesProgressPercent()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        video.SetProgress(75);
        video.ProgressPercent.Should().Be(75);
        video.UpdatedAt.Should().NotBeNull();
    }

    // ─────────────── SetDuration ───────────────

    [Fact]
    public void SetDuration_WhenZero_ThrowsArgumentOutOfRangeException()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        var act = () => video.SetDuration(0);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("durationSec");
    }

    [Fact]
    public void SetDuration_WhenNegative_ThrowsArgumentOutOfRangeException()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        var act = () => video.SetDuration(-10.5);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("durationSec");
    }

    [Fact]
    public void SetDuration_WhenValid_SetsDurationSec()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        video.SetDuration(120.5);
        video.DurationSec.Should().Be(120.5);
    }

    // ─────────────── SetFrameIntervalSec ───────────────

    [Fact]
    public void SetFrameIntervalSec_WhenZero_ThrowsArgumentOutOfRangeException()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        var act = () => video.SetFrameIntervalSec(0);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("frameIntervalSec");
    }

    [Fact]
    public void SetFrameIntervalSec_WhenValid_SetsFrameIntervalSec()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        video.SetFrameIntervalSec(2.5);
        video.FrameIntervalSec.Should().Be(2.5);
    }

    // ─────────────── SetParallelChunks ───────────────

    [Fact]
    public void SetParallelChunks_WhenZero_ThrowsArgumentOutOfRangeException()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        var act = () => video.SetParallelChunks(0);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("value");
    }

    [Fact]
    public void SetParallelChunks_WhenAbove100_ThrowsArgumentOutOfRangeException()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        var act = () => video.SetParallelChunks(101);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("value");
    }

    [Fact]
    public void SetParallelChunks_WhenValid_SetsParallelChunks()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        video.SetParallelChunks(10);
        video.ParallelChunks.Should().Be(10);
    }

    // ─────────────── MarkAsFailed ───────────────

    [Fact]
    public void MarkAsFailed_WithErrorCode_SetsAllFailureFields()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        video.MarkAsFailed("Processing error", "ERR_001");

        video.Status.Should().Be(VideoStatus.Failed);
        video.ErrorMessage.Should().Be("Processing error");
        video.ErrorCode.Should().Be("ERR_001");
        video.LastFailedAt.Should().NotBeNull();
        video.UpdatedAt.Should().NotBeNull();
    }

    // ─────────────── MarkAsCompleted ───────────────

    [Fact]
    public void MarkAsCompleted_WhenAlreadyHasProcessingCompletedAt_DoesNotOverwrite()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        video.UpdateStatus(VideoStatus.Completed);
        var firstTimestamp = video.ProcessingCompletedAt;

        // Second MarkAsCompleted should NOT overwrite ProcessingCompletedAt
        video.MarkAsCompleted();

        video.ProcessingCompletedAt.Should().Be(firstTimestamp);
    }

    [Fact]
    public void MarkAsCompleted_SetsProgressTo100()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        video.SetProgress(80);
        video.MarkAsCompleted();

        video.ProgressPercent.Should().Be(100);
        video.Status.Should().Be(VideoStatus.Completed);
    }

    // ─────────────── ApplyTransitionTimestamps ───────────────

    [Fact]
    public void ApplyTransitionTimestamps_ProcessingImages_WhenAlreadySet_DoesNotOverwrite()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        video.UpdateStatus(VideoStatus.ProcessingImages);
        var firstTimestamp = video.ProcessingStartedAt;

        // Applying again should NOT overwrite
        video.ApplyTransitionTimestamps(VideoStatus.UploadPending, VideoStatus.ProcessingImages);

        video.ProcessingStartedAt.Should().Be(firstTimestamp);
    }

    [Fact]
    public void ApplyTransitionTimestamps_GeneratingZip_WhenAlreadySet_DoesNotOverwrite()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        video.UpdateStatus(VideoStatus.GeneratingZip);
        var firstTimestamp = video.ImagesProcessingCompletedAt;

        video.ApplyTransitionTimestamps(VideoStatus.ProcessingImages, VideoStatus.GeneratingZip);

        video.ImagesProcessingCompletedAt.Should().Be(firstTimestamp);
    }

    [Fact]
    public void ApplyTransitionTimestamps_ToFailed_AlwaysUpdatesLastFailedAt()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        video.UpdateStatus(VideoStatus.Failed);
        var firstTimestamp = video.LastFailedAt;

        System.Threading.Thread.Sleep(10);
        video.ApplyTransitionTimestamps(VideoStatus.ProcessingImages, VideoStatus.Failed);

        video.LastFailedAt.Should().NotBe(firstTimestamp);
    }

    // ─────────────── FromMerge ───────────────

    [Fact]
    public void FromMerge_WhenPatchHasStatus_UsesNewStatus()
    {
        var existing = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        var patch = EmptyPatch() with { Status = VideoStatus.ProcessingImages };

        var merged = Video.FromMerge(existing, patch);

        merged.Status.Should().Be(VideoStatus.ProcessingImages);
        merged.VideoId.Should().Be(existing.VideoId);
    }

    [Fact]
    public void FromMerge_WhenPatchHasNullStatus_KeepsExistingStatus()
    {
        var existing = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        var patch = EmptyPatch();

        var merged = Video.FromMerge(existing, patch);

        merged.Status.Should().Be(VideoStatus.UploadPending);
    }

    [Fact]
    public void FromMerge_WhenPatchHasProgressPercent_UsesNewProgress()
    {
        var existing = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        var patch = EmptyPatch() with { ProgressPercent = 75 };

        var merged = Video.FromMerge(existing, patch);

        merged.ProgressPercent.Should().Be(75);
    }

    [Fact]
    public void FromMerge_WhenPatchHasErrorMessage_UsesNewErrorMessage()
    {
        var existing = new Video(Guid.NewGuid(), "video.mp4", "video/mp4", 1024);
        var patch = EmptyPatch() with { ErrorMessage = "New error", ErrorCode = "ERR_002" };

        var merged = Video.FromMerge(existing, patch);

        merged.ErrorMessage.Should().Be("New error");
        merged.ErrorCode.Should().Be("ERR_002");
    }

    private static VideoUpdateValues EmptyPatch() =>
        new(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
}
