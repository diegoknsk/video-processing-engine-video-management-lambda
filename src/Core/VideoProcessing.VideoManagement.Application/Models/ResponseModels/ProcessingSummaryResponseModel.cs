using System.ComponentModel;

namespace VideoProcessing.VideoManagement.Application.Models.ResponseModels;

/// <summary>
/// Resumo de processamento com chunks na resposta da API.
/// </summary>
public record ProcessingSummaryResponseModel
{
    [Description("Chunks indexados por ChunkId")]
    public IReadOnlyDictionary<string, ChunkInfoResponseModel> Chunks { get; init; } = new Dictionary<string, ChunkInfoResponseModel>();
}
