using System.Text.Json.Serialization;
using VideoProcessing.VideoManagement.Application.Models.InputModels;

namespace VideoProcessing.VideoManagement.LambdaUpdateVideo.Models;

/// <summary>
/// Event shape da Lambda de update de vídeo: mesmo contrato do PATCH (UpdateVideoInputModel) com videoId no evento.
/// Reutiliza UpdateVideoInputModel; apenas adiciona VideoId para a borda Lambda (invocação direta, API Gateway, SQS).
/// </summary>
public record UpdateVideoLambdaEvent : UpdateVideoInputModel
{
    /// <summary>Identificador do vídeo a atualizar (obrigatório). Na API vem na rota; na Lambda vem no payload.</summary>
    [JsonPropertyName("videoId")]
    public Guid VideoId { get; init; }
}
