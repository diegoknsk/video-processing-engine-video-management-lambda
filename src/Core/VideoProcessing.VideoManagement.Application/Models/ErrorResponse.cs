namespace VideoProcessing.VideoManagement.Application.Models;

/// <summary>
/// Resposta padronizada de erro (RFC 7807-style) retornada em 4xx e 5xx.
/// </summary>
/// <param name="Type">URI que identifica o tipo do problema.</param>
/// <param name="Title">Título legível do tipo de erro.</param>
/// <param name="Status">Código HTTP de status.</param>
/// <param name="Detail">Detalhe opcional da falha.</param>
/// <param name="TraceId">Identificador de rastreamento da requisição.</param>
public record ErrorResponse(
    string Type,
    string Title,
    int Status,
    string? Detail,
    string? TraceId
);
