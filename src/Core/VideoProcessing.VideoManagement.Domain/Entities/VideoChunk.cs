namespace VideoProcessing.VideoManagement.Domain.Entities;

/// <summary>
/// Constantes relacionadas a chunks de vídeo.
/// </summary>
public static class VideoChunkConstants
{
    /// <summary>ChunkId do item especial de finalização (geração de ZIP) — não contabilizado em totalChunks.</summary>
    public const string FinalizeChunkId = "finalize";

    /// <summary>Status de chunk concluído — usado no caminho fan-out onde a mensagem só é enviada após o chunk terminar.</summary>
    public const string StatusCompleted = "completed";
}

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
