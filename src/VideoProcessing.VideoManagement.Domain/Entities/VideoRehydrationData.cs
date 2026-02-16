using VideoProcessing.VideoManagement.Domain.Enums;

namespace VideoProcessing.VideoManagement.Domain.Entities;

/// <summary>
/// Dados para reidratação da entidade Video a partir da persistência.
/// </summary>
internal record VideoRehydrationData(
    Guid VideoId,
    Guid UserId,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    double? DurationSec,
    VideoStatus Status,
    ProcessingMode ProcessingMode,
    int ProgressPercent,
    string? S3BucketVideo,
    string? S3KeyVideo,
    string? S3BucketZip,
    string? S3KeyZip,
    string? S3BucketFrames,
    string? FramesPrefix,
    string? StepExecutionArn,
    string? ErrorMessage,
    string? ErrorCode,
    string? ClientRequestId,
    int? ChunkCount,
    double? ChunkDurationSec,
    DateTime? UploadIssuedAt,
    DateTime? UploadUrlExpiresAt,
    int? FramesProcessed,
    DateTime? FinalizedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int? Version);
