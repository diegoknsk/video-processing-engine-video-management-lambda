using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VideoProcessing.VideoManagement.Application.Models.InputModels;

/// <summary>
/// Contrato de entrada para registro de novo vídeo e obtenção de presigned URL de upload.
/// </summary>
public record UploadVideoInputModel
{
    /// <summary>Nome original do arquivo de vídeo enviado pelo cliente.</summary>
    [Required(ErrorMessage = "OriginalFileName é obrigatório")]
    [Description("Nome original do arquivo de vídeo")]
    public required string OriginalFileName { get; init; }

    /// <summary>Tipo MIME do conteúdo (ex.: video/mp4).</summary>
    [Required(ErrorMessage = "ContentType é obrigatório")]
    [Description("Tipo MIME do conteúdo (ex.: video/mp4)")]
    public required string ContentType { get; init; }

    /// <summary>Tamanho do arquivo em bytes.</summary>
    [Required]
    [Description("Tamanho do arquivo em bytes")]
    public long SizeBytes { get; init; }

    /// <summary>Duração do vídeo em segundos (opcional).</summary>
    [Description("Duração do vídeo em segundos")]
    public double? DurationSec { get; init; }

    /// <summary>Identificador do cliente para idempotência (opcional).</summary>
    [Description("Identificador do cliente para idempotência")]
    public string? ClientRequestId { get; init; }
}
