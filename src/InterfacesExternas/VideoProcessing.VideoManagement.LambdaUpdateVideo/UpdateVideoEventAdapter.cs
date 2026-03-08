using System.Text.Json;
using Microsoft.Extensions.Logging;
using VideoProcessing.VideoManagement.LambdaUpdateVideo.Models;

namespace VideoProcessing.VideoManagement.LambdaUpdateVideo;

/// <summary>
/// Detecta se o evento é envelope SQS (Records[].body) ou JSON direto do DTO e extrai uma lista de <see cref="UpdateVideoLambdaEvent"/>.
/// </summary>
public sealed class UpdateVideoEventAdapter : IUpdateVideoEventAdapter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ILogger<UpdateVideoEventAdapter> _logger;

    public UpdateVideoEventAdapter(ILogger<UpdateVideoEventAdapter> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public IReadOnlyList<UpdateVideoLambdaEvent> FromRawEvent(JsonDocument rawEvent)
    {
        if (rawEvent == null)
        {
            _logger.LogWarning("Raw event is null");
            return Array.Empty<UpdateVideoLambdaEvent>();
        }

        JsonElement root = rawEvent.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            _logger.LogWarning("Raw event root is not an object: {ValueKind}", root.ValueKind);
            return Array.Empty<UpdateVideoLambdaEvent>();
        }

        return TryParseSqsOrEmpty(root);
    }

    private static bool IsSqsEnvelope(JsonElement root)
    {
        if (!root.TryGetProperty("Records", out var records))
            return false;
        if (records.ValueKind != JsonValueKind.Array)
            return false;
        if (records.GetArrayLength() == 0)
            return false;
        JsonElement first = records[0];
        return first.TryGetProperty("body", out _);
    }

    /// <summary>
    /// Records presente mas vazio → payload SQS inválido (sem mensagens), retorna lista vazia.
    /// </summary>
    private IReadOnlyList<UpdateVideoLambdaEvent> TryParseSqsOrEmpty(JsonElement root)
    {
        if (root.TryGetProperty("Records", out var records) && records.ValueKind == JsonValueKind.Array && records.GetArrayLength() == 0)
        {
            _logger.LogWarning("SQS envelope has empty Records array");
            return Array.Empty<UpdateVideoLambdaEvent>();
        }
        if (IsSqsEnvelope(root))
            return ParseSqsRecords(root);
        return ParseDirectPayload(root);
    }

    private IReadOnlyList<UpdateVideoLambdaEvent> ParseSqsRecords(JsonElement root)
    {
        var records = root.GetProperty("Records");
        var list = new List<UpdateVideoLambdaEvent>(records.GetArrayLength());
        for (int i = 0; i < records.GetArrayLength(); i++)
        {
            JsonElement record = records[i];
            if (!record.TryGetProperty("body", out var bodyProp))
            {
                _logger.LogWarning("SQS record at index {Index} has no body", i);
                return Array.Empty<UpdateVideoLambdaEvent>();
            }
            string? body = bodyProp.GetString();
            if (string.IsNullOrWhiteSpace(body))
            {
                _logger.LogWarning("SQS record at index {Index} has null or empty body", i);
                return Array.Empty<UpdateVideoLambdaEvent>();
            }
            try
            {
                var evt = JsonSerializer.Deserialize<UpdateVideoLambdaEvent>(body, JsonOptions);
                if (evt == null)
                {
                    _logger.LogWarning("SQS record at index {Index} deserialized to null", i);
                    return Array.Empty<UpdateVideoLambdaEvent>();
                }
                list.Add(evt);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "SQS record at index {Index} has invalid JSON body", i);
                return Array.Empty<UpdateVideoLambdaEvent>();
            }
        }
        return list;
    }

    private IReadOnlyList<UpdateVideoLambdaEvent> ParseDirectPayload(JsonElement root)
    {
        try
        {
            var evt = JsonSerializer.Deserialize<UpdateVideoLambdaEvent>(root, JsonOptions);
            if (evt == null)
            {
                _logger.LogWarning("Direct payload deserialized to null");
                return Array.Empty<UpdateVideoLambdaEvent>();
            }
            return new[] { evt };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Direct payload is not valid UpdateVideoLambdaEvent JSON");
            return Array.Empty<UpdateVideoLambdaEvent>();
        }
    }
}
