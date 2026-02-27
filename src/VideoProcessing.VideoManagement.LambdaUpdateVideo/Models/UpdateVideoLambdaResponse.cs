using System.Text.Json.Serialization;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;

namespace VideoProcessing.VideoManagement.LambdaUpdateVideo.Models;

/// <summary>
/// Resposta da Lambda de update de vídeo. Sucesso: StatusCode 200 e Video preenchido. Erro: StatusCode 400/404/409 e ErrorCode/ErrorMessage.
/// </summary>
public record UpdateVideoLambdaResponse
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; init; }

    [JsonPropertyName("video")]
    public VideoResponseModel? Video { get; init; }

    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; init; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; init; }

    public static UpdateVideoLambdaResponse Ok(VideoResponseModel video) =>
        new() { StatusCode = 200, Video = video };

    public static UpdateVideoLambdaResponse ValidationError(string message) =>
        new() { StatusCode = 400, ErrorCode = "ValidationFailed", ErrorMessage = message };

    public static UpdateVideoLambdaResponse NotFound(string message = "Vídeo não encontrado.") =>
        new() { StatusCode = 404, ErrorCode = "NotFound", ErrorMessage = message };

    public static UpdateVideoLambdaResponse Conflict(string message) =>
        new() { StatusCode = 409, ErrorCode = "UpdateConflict", ErrorMessage = message };
}
