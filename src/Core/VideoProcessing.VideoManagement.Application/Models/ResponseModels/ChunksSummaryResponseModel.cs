namespace VideoProcessing.VideoManagement.Application.Models.ResponseModels;

/// <summary>
/// Resumo de contagem de chunks por status para exibição na API.
/// </summary>
public record ChunksSummaryResponseModel(
    int Total,
    int Completed,
    int Processing,
    int Failed,
    int Pending);
