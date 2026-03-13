namespace VideoProcessing.VideoManagement.Application.Models;

/// <summary>
/// Resultado do cálculo de progresso baseado em chunks (percentual, stage e se há chunks).
/// </summary>
public record ChunkProgressResult(
    int ProgressPercent,
    string CurrentStage,
    bool HasChunks);
