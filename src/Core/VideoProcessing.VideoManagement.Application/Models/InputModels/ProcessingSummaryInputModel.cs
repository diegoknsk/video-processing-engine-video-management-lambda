using System.ComponentModel;

namespace VideoProcessing.VideoManagement.Application.Models.InputModels;

/// <summary>
/// DTO de entrada para resumo de processamento (chunks a serem mergeados incrementalmente).
/// </summary>
public record ProcessingSummaryInputModel
{
    [Description("Chunks indexados por ChunkId")]
    public IReadOnlyDictionary<string, ChunkInfoInputModel>? Chunks { get; init; }
}
