using System.Text.Json;
using VideoProcessing.VideoManagement.LambdaUpdateVideo.Models;

namespace VideoProcessing.VideoManagement.LambdaUpdateVideo;

/// <summary>
/// Converte o evento bruto da Lambda (envelope SQS ou JSON direto do DTO de update) em uma lista de <see cref="UpdateVideoLambdaEvent"/>.
/// </summary>
public interface IUpdateVideoEventAdapter
{
    /// <summary>
    /// Extrai um ou mais eventos de update a partir do payload bruto.
    /// Se o evento for formato SQS (Records[].body), cada body é desserializado; caso contrário, o payload é tratado como um único DTO.
    /// Retorna lista vazia se o payload for inválido ou malformado.
    /// </summary>
    IReadOnlyList<UpdateVideoLambdaEvent> FromRawEvent(JsonDocument rawEvent);
}
