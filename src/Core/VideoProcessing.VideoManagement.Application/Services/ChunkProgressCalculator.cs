using VideoProcessing.VideoManagement.Application.Models;
using VideoProcessing.VideoManagement.Domain.Enums;

namespace VideoProcessing.VideoManagement.Application.Services;

/// <summary>
/// Calcula progresso e stage do vídeo a partir dos chunks (regras de negócio centralizadas).
/// </summary>
public sealed class ChunkProgressCalculator : IChunkProgressCalculator
{
    /// <inheritdoc />
    public ChunkProgressResult Calculate(VideoStatus videoStatus, ChunkStatusSummary? summary)
    {
        var hasChunks = summary is not null && summary.Total > 0;

        var progressPercent = ComputeProgressPercent(videoStatus, summary, hasChunks);
        var currentStage = ComputeCurrentStage(videoStatus, hasChunks);

        return new ChunkProgressResult(progressPercent, currentStage, hasChunks);
    }

    private static int ComputeProgressPercent(VideoStatus videoStatus, ChunkStatusSummary? summary, bool hasChunks)
    {
        if (videoStatus == VideoStatus.Completed)
            return 100;

        if (!hasChunks || summary is null)
            return -1; // sinaliza "usar valor salvo"

        var total = summary.Total;
        if (total <= 0)
            return -1;

        if (string.Equals(summary.FinalizeStatus, "completed", StringComparison.OrdinalIgnoreCase))
            return 100;

        return (int)Math.Floor((summary.Completed * 100.0) / total);
    }

    private static string ComputeCurrentStage(VideoStatus videoStatus, bool hasChunks)
    {
        return videoStatus switch
        {
            VideoStatus.UploadPending => "Upload pendente",
            VideoStatus.ProcessingImages => hasChunks ? "Processando chunks" : "Processando",
            VideoStatus.GeneratingZip => "Gerando ZIP",
            VideoStatus.Completed => "Concluído",
            VideoStatus.Failed => "Falhou",
            VideoStatus.Cancelled => "Cancelado",
            _ => videoStatus.ToString()
        };
    }
}
