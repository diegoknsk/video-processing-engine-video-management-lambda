using System.Text.Json;
using VideoProcessing.VideoManagement.Domain.Entities;
using VideoProcessing.VideoManagement.Domain.Enums;
using VideoProcessing.VideoManagement.Infra.Data.Repositories;

namespace VideoProcessing.VideoManagement.Infra.Data.Mappers;

public static class VideoMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
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
            FrameIntervalSec = video.FrameIntervalSec,
            Status = video.Status.ToString(),
            ProcessingMode = video.ProcessingMode.ToString(),
            ProgressPercent = video.ProgressPercent,
            S3BucketVideo = video.S3BucketVideo,
            S3KeyVideo = video.S3KeyVideo,
            S3BucketZip = video.S3BucketZip,
            S3KeyZip = video.S3KeyZip,
            ZipBucket = video.ZipBucket,
            ZipKey = video.ZipKey,
            ZipFileName = video.ZipFileName,
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
            ParallelChunks = video.ParallelChunks,
            ProcessingSummaryJson = video.ProcessingSummary is null ? null : JsonSerializer.Serialize(video.ProcessingSummary.Chunks, JsonOptions),
            ProcessingStartedAt = video.ProcessingStartedAt?.ToString("O"),
            ImagesProcessingCompletedAt = video.ImagesProcessingCompletedAt?.ToString("O"),
            ProcessingCompletedAt = video.ProcessingCompletedAt?.ToString("O"),
            LastFailedAt = video.LastFailedAt?.ToString("O"),
            LastCancelledAt = video.LastCancelledAt?.ToString("O"),
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
            FrameIntervalSec: entity.FrameIntervalSec,
            Status: ParseStatus(entity.Status),
            ProcessingMode: Enum.Parse<ProcessingMode>(entity.ProcessingMode),
            ProgressPercent: entity.ProgressPercent,
            S3BucketVideo: entity.S3BucketVideo,
            S3KeyVideo: entity.S3KeyVideo,
            S3BucketZip: entity.S3BucketZip,
            S3KeyZip: entity.S3KeyZip,
            ZipBucket: entity.ZipBucket,
            ZipKey: entity.ZipKey,
            ZipFileName: entity.ZipFileName,
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
            ParallelChunks: entity.ParallelChunks,
            ProcessingSummary: ParseProcessingSummary(entity.ProcessingSummaryJson),
            ProcessingStartedAt: ParseDateTime(entity.ProcessingStartedAt),
            ImagesProcessingCompletedAt: ParseDateTime(entity.ImagesProcessingCompletedAt),
            ProcessingCompletedAt: ParseDateTime(entity.ProcessingCompletedAt),
            LastFailedAt: ParseDateTime(entity.LastFailedAt),
            LastCancelledAt: ParseDateTime(entity.LastCancelledAt),
            CreatedAt: DateTime.Parse(entity.CreatedAt),
            UpdatedAt: ParseDateTime(entity.UpdatedAt),
            Version: entity.Version);

        return Video.FromPersistence(data);
    }

    private static DateTime? ParseDateTime(string? value) =>
        string.IsNullOrEmpty(value) ? null : DateTime.Parse(value);

    private static ProcessingSummary? ParseProcessingSummary(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            var chunks = JsonSerializer.Deserialize<Dictionary<string, ChunkInfo>>(json, JsonOptions);
            return chunks is null || chunks.Count == 0 ? null : new ProcessingSummary(chunks);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parseia o status persistido, aceitando valores legados (Pending, Uploading, Processing) para compatibilidade com dados já existentes no DynamoDB.
    /// </summary>
    private static VideoStatus ParseStatus(string value)
    {
        if (Enum.TryParse<VideoStatus>(value, ignoreCase: true, out var status))
            return status;
        return value switch
        {
            "Pending" => VideoStatus.UploadPending,
            "Uploading" => VideoStatus.UploadPending,
            "Processing" => VideoStatus.ProcessingImages,
            _ => throw new ArgumentException($"Unknown or legacy status value: {value}.", nameof(value))
        };
    }
}
