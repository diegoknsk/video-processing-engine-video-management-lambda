using VideoProcessing.VideoManagement.Application.Extensions;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Domain.Entities;

namespace VideoProcessing.VideoManagement.Application.Models.Mappers;

/// <summary>
/// Mapeia entidade Video do Domain para VideoResponseModel da API.
/// </summary>
public static class VideoResponseModelMapper
{
    /// <summary>Converte Video para VideoResponseModel.</summary>
    public static VideoResponseModel ToResponseModel(Video video)
    {
        return new VideoResponseModel
        {
            VideoId = video.VideoId,
            UserId = video.UserId,
            UserEmail = video.UserEmail,
            OriginalFileName = video.OriginalFileName,
            ContentType = video.ContentType,
            SizeBytes = video.SizeBytes,
            DurationSec = video.DurationSec,
            FrameIntervalSec = video.FrameIntervalSec,
            Status = video.Status,
            StatusDescription = video.Status.ToFriendlyName(),
            ProcessingMode = video.ProcessingMode,
            ProgressPercent = video.ProgressPercent,
            S3BucketVideo = video.S3BucketVideo,
            S3KeyVideo = video.S3KeyVideo,
            S3BucketZip = video.S3BucketZip,
            S3KeyZip = video.S3KeyZip,
            ZipFileName = video.ZipFileName,
            S3BucketFrames = video.S3BucketFrames,
            FramesPrefix = video.FramesPrefix,
            StepExecutionArn = video.StepExecutionArn,
            ErrorMessage = video.ErrorMessage,
            ErrorCode = video.ErrorCode,
            ClientRequestId = video.ClientRequestId,
            ChunkCount = video.ChunkCount,
            ChunkDurationSec = video.ChunkDurationSec,
            UploadIssuedAt = video.UploadIssuedAt,
            UploadUrlExpiresAt = video.UploadUrlExpiresAt,
            FramesProcessed = video.FramesProcessed,
            FinalizedAt = video.FinalizedAt,
            ParallelChunks = video.ParallelChunks,
            ProcessingSummary = video.ProcessingSummary is null ? null : ToProcessingSummaryResponse(video.ProcessingSummary),
            ProcessingStartedAt = video.ProcessingStartedAt,
            ImagesProcessingCompletedAt = video.ImagesProcessingCompletedAt,
            ProcessingCompletedAt = video.ProcessingCompletedAt,
            LastFailedAt = video.LastFailedAt,
            LastCancelledAt = video.LastCancelledAt,
            CreatedAt = video.CreatedAt,
            UpdatedAt = video.UpdatedAt,
            Version = video.Version
        };
    }

    private static ProcessingSummaryResponseModel ToProcessingSummaryResponse(Domain.Entities.ProcessingSummary summary)
    {
        var chunks = summary.Chunks.ToDictionary(
            kv => kv.Key,
            kv => new ChunkInfoResponseModel
            {
                ChunkId = kv.Value.ChunkId,
                StartSec = kv.Value.StartSec,
                EndSec = kv.Value.EndSec,
                IntervalSec = kv.Value.IntervalSec,
                ManifestPrefix = kv.Value.ManifestPrefix,
                FramesPrefix = kv.Value.FramesPrefix
            });
        return new ProcessingSummaryResponseModel { Chunks = chunks };
    }
}
