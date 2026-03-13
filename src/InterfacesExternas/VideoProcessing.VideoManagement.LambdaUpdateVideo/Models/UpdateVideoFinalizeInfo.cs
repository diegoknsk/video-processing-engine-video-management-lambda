using System.Text.Json.Serialization;

namespace VideoProcessing.VideoManagement.LambdaUpdateVideo.Models;

/// <summary>
/// Metadados de finalização recebidos no campo <c>finalize</c> do evento Lambda de update de vídeo.
/// Presente apenas em eventos de finalização (status=Completed/GeneratingZip).
/// </summary>
public record UpdateVideoFinalizeInfo
{
    [JsonPropertyName("videoId")]
    public Guid? VideoId { get; init; }

    [JsonPropertyName("framesBucket")]
    public string? FramesBucket { get; init; }

    [JsonPropertyName("framesBasePrefix")]
    public string? FramesBasePrefix { get; init; }

    [JsonPropertyName("outputBucket")]
    public string? OutputBucket { get; init; }

    [JsonPropertyName("outputBasePrefix")]
    public string? OutputBasePrefix { get; init; }

    [JsonPropertyName("ordenaAutomaticamente")]
    public bool OrdenaAutomaticamente { get; init; }
}
