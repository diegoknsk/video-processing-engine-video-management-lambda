using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VideoProcessing.VideoManagement.LambdaUpdateVideo.Models;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace VideoProcessing.VideoManagement.LambdaUpdateVideo;

/// <summary>
/// Handler da Lambda de atualização de vídeo (Lambda pura — sem AddAWSLambdaHosting).
/// Event shape: UpdateVideoLambdaEvent = UpdateVideoInputModel + videoId (mesmo contrato do PATCH).
/// </summary>
public class Function
{
    private readonly IUpdateVideoHandler _handler;
    private readonly ILogger<Function> _logger;

    public Function()
        : this(Startup.BuildServiceProvider())
    {
    }

    internal Function(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<IUpdateVideoHandler>();
        _logger = serviceProvider.GetRequiredService<ILogger<Function>>();
    }

    /// <summary>
    /// Handler principal: recebe o evento (videoId + UpdateVideoInputModel), valida, executa Use Case e retorna resposta.
    /// </summary>
    public async Task<UpdateVideoLambdaResponse> Handler(UpdateVideoLambdaEvent request, ILambdaContext context)
    {
        _logger.LogInformation("UpdateVideo Lambda invoked for VideoId={VideoId}, UserId={UserId}", request.VideoId, request.UserId);
        return await _handler.HandleAsync(request, CancellationToken.None);
    }
}
