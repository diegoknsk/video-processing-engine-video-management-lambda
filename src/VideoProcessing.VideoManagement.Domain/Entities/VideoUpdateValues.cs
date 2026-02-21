using VideoProcessing.VideoManagement.Domain.Enums;

namespace VideoProcessing.VideoManagement.Domain.Entities;

/// <summary>
/// Valores opcionais para atualização parcial de um vídeo. Apenas campos não nulos são aplicados.
/// </summary>
public record VideoUpdateValues(
    VideoStatus? Status,
    int? ProgressPercent,
    string? ErrorMessage,
    string? ErrorCode,
    string? FramesPrefix,
    string? S3KeyZip,
    string? S3BucketFrames,
    string? S3BucketZip,
    string? StepExecutionArn);
