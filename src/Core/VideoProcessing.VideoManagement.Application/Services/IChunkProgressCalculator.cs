using VideoProcessing.VideoManagement.Application.Models;
using VideoProcessing.VideoManagement.Domain.Enums;

namespace VideoProcessing.VideoManagement.Application.Services;

/// <summary>
/// Calcula progresso e stage do vídeo a partir do status e do resumo de chunks.
/// </summary>
public interface IChunkProgressCalculator
{
    /// <summary>Calcula percentual de progresso, stage amigável e se há chunks (para fallback).</summary>
    /// <param name="videoStatus">Status atual do vídeo.</param>
    /// <param name="summary">Resumo de chunks (null ou Total=0 para vídeos sem chunks).</param>
    /// <returns>ProgressPercent: use valor salvo quando &lt; 0 (HasChunks = false).</returns>
    ChunkProgressResult Calculate(VideoStatus videoStatus, ChunkStatusSummary? summary);
}
