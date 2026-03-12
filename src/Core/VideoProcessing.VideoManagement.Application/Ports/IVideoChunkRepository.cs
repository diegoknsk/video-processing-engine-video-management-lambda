using VideoProcessing.VideoManagement.Domain.Entities;

namespace VideoProcessing.VideoManagement.Application.Ports;

/// <summary>
/// Port para persistência de chunks de vídeo na tabela dedicada DynamoDB.
/// </summary>
public interface IVideoChunkRepository
{
    /// <summary>Insere ou atualiza o registro do chunk (PutItem idempotente).</summary>
    Task UpsertAsync(VideoChunk chunk, CancellationToken ct = default);

    /// <summary>Conta chunks com status concluído (ex.: "completed") para o vídeo.</summary>
    Task<int> CountProcessedAsync(string videoId, CancellationToken ct = default);
}
