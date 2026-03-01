using VideoProcessing.VideoManagement.Domain.Entities;
using VideoProcessing.VideoManagement.Domain.Enums;
using VideoProcessing.VideoManagement.Infra.Data.Repositories;

namespace VideoProcessing.VideoManagement.Infra.Data.Mappers;

public static class VideoMapper
{
    public static VideoEntity ToEntity(Video video, string? clientRequestId = null)
    {
        var entity = new VideoEntity
        {
            Pk = $"USER#{video.UserId}",
            Sk = $"VIDEO#{video.VideoId}",
            VideoId = video.VideoId.ToString(),
            UserId = video.UserId.ToString(),
            OriginalFileName = video.OriginalFileName,
            ContentType = video.ContentType,
            SizeBytes = video.SizeBytes,
            DurationSec = video.DurationSec,
            Status = video.Status.ToString(),
            ProcessingMode = video.ProcessingMode.ToString(),
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
            ClientRequestId = clientRequestId ?? video.ClientRequestId,
            ChunkCount = video.ChunkCount,
            ChunkDurationSec = video.ChunkDurationSec,
            UploadIssuedAt = video.UploadIssuedAt?.ToString("O"),
            UploadUrlExpiresAt = video.UploadUrlExpiresAt?.ToString("O"),
            FramesProcessed = video.FramesProcessed,
            FinalizedAt = video.FinalizedAt?.ToString("O"),
            CreatedAt = video.CreatedAt.ToString("O"),
            UpdatedAt = video.UpdatedAt?.ToString("O"),
            Version = video.Version
        };

        if (!string.IsNullOrEmpty(entity.ClientRequestId))
        {
            entity.Gsi1Pk = $"USER#{video.UserId}";
            entity.Gsi1Sk = $"CLIENT_REQUEST#{entity.ClientRequestId}";
        }

        return entity;
    }

    public static Video ToDomain(VideoEntity entity)
    {
        var data = new VideoRehydrationData(
            VideoId: Guid.Parse(entity.VideoId),
            UserId: Guid.Parse(entity.UserId),
            OriginalFileName: entity.OriginalFileName,
            ContentType: entity.ContentType,
            SizeBytes: entity.SizeBytes,
            DurationSec: entity.DurationSec,
            Status: Enum.Parse<VideoStatus>(entity.Status),
            ProcessingMode: Enum.Parse<ProcessingMode>(entity.ProcessingMode),
            ProgressPercent: entity.ProgressPercent,
            S3BucketVideo: entity.S3BucketVideo,
            S3KeyVideo: entity.S3KeyVideo,
            S3BucketZip: entity.S3BucketZip,
            S3KeyZip: entity.S3KeyZip,
            S3BucketFrames: entity.S3BucketFrames,
            FramesPrefix: entity.FramesPrefix,
            StepExecutionArn: entity.StepExecutionArn,
            ErrorMessage: entity.ErrorMessage,
            ErrorCode: entity.ErrorCode,
            ClientRequestId: entity.ClientRequestId,
            ChunkCount: entity.ChunkCount,
            ChunkDurationSec: entity.ChunkDurationSec,
            UploadIssuedAt: ParseDateTime(entity.UploadIssuedAt),
            UploadUrlExpiresAt: ParseDateTime(entity.UploadUrlExpiresAt),
            FramesProcessed: entity.FramesProcessed,
            FinalizedAt: ParseDateTime(entity.FinalizedAt),
            CreatedAt: DateTime.Parse(entity.CreatedAt),
            UpdatedAt: ParseDateTime(entity.UpdatedAt),
            Version: entity.Version);

        return Video.FromPersistence(data);
    }

    private static DateTime? ParseDateTime(string? value) =>
        string.IsNullOrEmpty(value) ? null : DateTime.Parse(value);
}
