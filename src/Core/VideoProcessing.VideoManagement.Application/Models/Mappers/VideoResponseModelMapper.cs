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
            OriginalFileName = video.OriginalFileName,
            ContentType = video.ContentType,
            SizeBytes = video.SizeBytes,
            DurationSec = video.DurationSec,
            Status = video.Status,
            ProcessingMode = video.ProcessingMode,
            ProgressPercent = video.ProgressPercent,
            S3BucketVideo = video.S3BucketVideo,
            S3KeyVideo = video.S3KeyVideo,
            S3BucketZip = video.S3BucketZip,
            S3KeyZip = video.S3KeyZip,
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
            CreatedAt = video.CreatedAt,
            UpdatedAt = video.UpdatedAt,
            Version = video.Version
        };
    }
}
