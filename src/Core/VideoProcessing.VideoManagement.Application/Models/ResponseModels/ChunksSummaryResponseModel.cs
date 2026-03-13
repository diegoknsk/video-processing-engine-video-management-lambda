namespace VideoProcessing.VideoManagement.Application.Models.ResponseModels;

/// <summary>
/// Resumo de contagem de chunks por status para exibição na API.
/// </summary>
public record ChunksSummaryResponseModel(
    int Total,
    int Completed,
    int Processing,
    int Failed,
    int Pending)
{
    /// <summary>Percentual de chunks concluídos (0-100).</summary>
    public int CompletionPercent => Total > 0 ? (int)Math.Floor(Completed * 100.0 / Total) : 0;
}
