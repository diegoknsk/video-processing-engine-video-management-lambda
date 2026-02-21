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

    /// <summary>Tamanho do arquivo em KB (quilobytes).</summary>
    [Required]
    [Description("Tamanho do arquivo em KB (quilobytes)")]
    public long SizeKb { get; init; }

    /// <summary>Duração do vídeo em segundos (opcional).</summary>
    [Description("Duração do vídeo em segundos")]
    public double? DurationSec { get; init; }

    /// <summary>Identificador do cliente para idempotência (opcional). Se informado, requisições com o mesmo valor para o mesmo usuário retornam o mesmo videoId (evita duplicar em retries). Para cada novo vídeo/arquivo use um UUID diferente ou omita o campo; não use um valor fixo (ex.: userId) para todos os uploads, senão apenas um vídeo será criado.</summary>
    [Description("UUID por requisição de upload. Um valor = um vídeo por usuário. Para múltiplos vídeos: envie um ClientRequestId diferente por arquivo ou omita.")]
    public string? ClientRequestId { get; init; }
}
