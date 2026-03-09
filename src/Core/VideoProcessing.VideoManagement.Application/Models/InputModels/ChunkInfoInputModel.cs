using System.ComponentModel;

namespace VideoProcessing.VideoManagement.Application.Models.InputModels;

/// <summary>
/// DTO de entrada para informações de um chunk de processamento.
/// </summary>
public record ChunkInfoInputModel
{
    [Description("Identificador do chunk")]
    public string ChunkId { get; init; } = string.Empty;

    [Description("Segundo de início no vídeo")]
    public double StartSec { get; init; }

    [Description("Segundo de fim no vídeo")]
    public double EndSec { get; init; }

    [Description("Intervalo em segundos entre frames")]
    public double IntervalSec { get; init; }

    [Description("Prefixo do manifest do chunk")]
    public string? ManifestPrefix { get; init; }

    [Description("Prefixo dos frames do chunk")]
    public string? FramesPrefix { get; init; }
}
