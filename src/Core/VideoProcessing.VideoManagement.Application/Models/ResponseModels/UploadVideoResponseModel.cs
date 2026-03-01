using System.ComponentModel;

namespace VideoProcessing.VideoManagement.Application.Models.ResponseModels;

/// <summary>
/// Resposta do registro de vídeo (POST /videos): identificador e URL presigned para upload.
/// </summary>
public record UploadVideoResponseModel
{
    /// <summary>Identificador único do vídeo criado.</summary>
    [Description("Identificador único do vídeo")]
    public Guid VideoId { get; init; }

    /// <summary>URL presigned para upload do arquivo no S3.</summary>
    [Description("URL presigned para upload no S3")]
    public required string UploadUrl { get; init; }

    /// <summary>Data/hora de expiração da URL de upload.</summary>
    [Description("Data/hora de expiração da URL")]
    public DateTime ExpiresAt { get; init; }
}
