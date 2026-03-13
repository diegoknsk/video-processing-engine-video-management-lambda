namespace VideoProcessing.VideoManagement.Domain.Entities;

/// <summary>
/// Representa o estado persistido de um chunk de processamento de vídeo (tabela de chunks DynamoDB).
/// </summary>
public record VideoChunk(
    string ChunkId,
    string VideoId,
    string Status,
    double StartSec,
    double EndSec,
    double IntervalSec,
    string? ManifestPrefix,
    string? FramesPrefix,
    DateTime? ProcessedAt,
    DateTime CreatedAt);
