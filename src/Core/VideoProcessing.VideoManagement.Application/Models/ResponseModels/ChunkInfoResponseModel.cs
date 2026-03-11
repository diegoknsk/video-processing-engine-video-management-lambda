using System.ComponentModel;

namespace VideoProcessing.VideoManagement.Application.Models.ResponseModels;

/// <summary>
/// Informações de um chunk na resposta da API.
/// </summary>
public record ChunkInfoResponseModel
{
    [Description("Identificador do chunk")]
    public string ChunkId { get; init; } = string.Empty;

    [Description("Segundo de início")]
    public double StartSec { get; init; }

    [Description("Segundo de fim")]
    public double EndSec { get; init; }

    [Description("Intervalo em segundos entre frames")]
    public double IntervalSec { get; init; }

    [Description("Prefixo do manifest")]
    public string ManifestPrefix { get; init; } = string.Empty;

    [Description("Prefixo dos frames")]
    public string FramesPrefix { get; init; } = string.Empty;
}
