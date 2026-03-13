namespace VideoProcessing.VideoManagement.Application.Models;

/// <summary>
/// Resumo de contagem de chunks por status para um vídeo (excluindo o item finalize do total).
/// </summary>
public record ChunkStatusSummary(
    int Total,
    int Completed,
    int Processing,
    int Failed,
    int Pending,
    string? FinalizeStatus);
