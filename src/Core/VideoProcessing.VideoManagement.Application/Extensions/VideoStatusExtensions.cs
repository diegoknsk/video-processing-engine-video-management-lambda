using VideoProcessing.VideoManagement.Domain.Enums;

namespace VideoProcessing.VideoManagement.Application.Extensions;

/// <summary>
/// Extensões para descrição amigável do status do vídeo (exposição em API).
/// </summary>
public static class VideoStatusExtensions
{
    /// <summary>
    /// Retorna a descrição amigável do status para exibição em listagem e detalhe do vídeo.
    /// </summary>
    public static string ToFriendlyName(this VideoStatus status) => status switch
    {
        VideoStatus.UploadPending => "Aguardando upload",
        VideoStatus.ProcessingImages => "Processando imagens",
        VideoStatus.GeneratingZip => "Gerando ZIP",
        VideoStatus.Completed => "Concluído",
        VideoStatus.Failed => "Falhou",
        VideoStatus.Cancelled => "Cancelado",
        _ => status.ToString()
    };
}
