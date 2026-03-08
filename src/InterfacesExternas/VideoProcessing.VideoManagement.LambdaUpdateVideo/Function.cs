using System.IO;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VideoProcessing.VideoManagement.LambdaUpdateVideo.Models;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace VideoProcessing.VideoManagement.LambdaUpdateVideo;

/// <summary>
/// Handler da Lambda de atualização de vídeo (Lambda pura — sem AddAWSLambdaHosting).
/// Aceita evento bruto: envelope SQS (Records[].body) ou JSON direto do DTO (UpdateVideoLambdaEvent).
/// O adapter normaliza para um ou mais eventos; cada um é processado pelo handler existente.
/// Política com múltiplos records: processa em ordem; retorna a primeira resposta de erro (4xx) ou a última de sucesso (200).
/// </summary>
public class Function
{
    private readonly IUpdateVideoHandler _handler;
    private readonly IUpdateVideoEventAdapter _adapter;
    private readonly ILogger<Function> _logger;

    public Function()
        : this(Startup.BuildServiceProvider())
    {
    }

    internal Function(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<IUpdateVideoHandler>();
        _adapter = serviceProvider.GetRequiredService<IUpdateVideoEventAdapter>();
        _logger = serviceProvider.GetRequiredService<ILogger<Function>>();
    }

    /// <summary>
    /// Handler principal: recebe evento bruto (SQS ou JSON direto), extrai um ou mais UpdateVideoLambdaEvent via adapter, processa cada um e retorna a resposta (última sucesso ou primeira erro).
    /// </summary>
    public async Task<UpdateVideoLambdaResponse> Handler(Stream rawEventStream, ILambdaContext context)
    {
        using JsonDocument rawEvent = await JsonDocument.ParseAsync(rawEventStream);
        IReadOnlyList<UpdateVideoLambdaEvent> events = _adapter.FromRawEvent(rawEvent);
        if (events.Count == 0)
        {
            _logger.LogWarning("Adapter returned no events (invalid or malformed payload)");
            return UpdateVideoLambdaResponse.ValidationError("Payload inválido ou malformado.");
        }

        UpdateVideoLambdaResponse? lastSuccess = null;
        for (int i = 0; i < events.Count; i++)
        {
            UpdateVideoLambdaEvent evt = events[i];
            _logger.LogInformation("UpdateVideo Lambda processing event {Index}/{Total} for VideoId={VideoId}, UserId={UserId}", i + 1, events.Count, evt.VideoId, evt.UserId);
            UpdateVideoLambdaResponse response = await _handler.HandleAsync(evt, CancellationToken.None);
            if (response.StatusCode >= 400)
                return response;
            lastSuccess = response;
        }

        return lastSuccess ?? UpdateVideoLambdaResponse.ValidationError("Nenhum evento processado.");
    }
}
