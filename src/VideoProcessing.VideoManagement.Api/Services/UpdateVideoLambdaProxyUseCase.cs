using System.Text;
using System.Text.Json;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Application.UseCases.UpdateVideo;
using VideoProcessing.VideoManagement.Api.Configuration;
using VideoProcessing.VideoManagement.Domain.Exceptions;
using VideoProcessing.VideoManagement.LambdaUpdateVideo.Models;

namespace VideoProcessing.VideoManagement.Api.Services;

/// <summary>
/// Implementa IUpdateVideoUseCase invocando a Lambda Update Video (proxy). Mantém o mesmo contrato para o cliente (PATCH).
/// </summary>
public class UpdateVideoLambdaProxyUseCase(
    AmazonLambdaClient lambdaClient,
    IOptions<LambdaUpdateVideoOptions> options) : IUpdateVideoUseCase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public async Task<VideoResponseModel?> ExecuteAsync(Guid videoId, UpdateVideoInputModel input, CancellationToken ct = default)
    {
        var evt = new UpdateVideoLambdaEvent
        {
            VideoId = videoId,
            UserId = input.UserId,
            Status = input.Status,
            ProgressPercent = input.ProgressPercent,
            ErrorMessage = input.ErrorMessage,
            ErrorCode = input.ErrorCode,
            FramesPrefix = input.FramesPrefix,
            S3KeyZip = input.S3KeyZip,
            S3BucketFrames = input.S3BucketFrames,
            S3BucketZip = input.S3BucketZip,
            StepExecutionArn = input.StepExecutionArn
        };
        var payload = JsonSerializer.Serialize(evt, JsonOptions);
        var invokeRequest = new InvokeRequest
        {
            FunctionName = options.Value.FunctionName,
            InvocationType = InvocationType.RequestResponse,
            Payload = payload
        };

        InvokeResponse invokeResponse;
        try
        {
            invokeResponse = await lambdaClient.InvokeAsync(invokeRequest, ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Falha ao invocar Lambda Update Video: {ex.Message}", ex);
        }

        if (invokeResponse.FunctionError != null)
        {
            throw new InvalidOperationException($"Lambda retornou erro: {invokeResponse.FunctionError}");
        }

        string responseBody;
        using (var reader = new StreamReader(invokeResponse.Payload))
            responseBody = await reader.ReadToEndAsync(ct);

        var response = JsonSerializer.Deserialize<UpdateVideoLambdaResponse>(responseBody, JsonOptions)
            ?? throw new InvalidOperationException("Resposta da Lambda inválida.");

        return response.StatusCode switch
        {
            200 => response.Video ?? throw new InvalidOperationException("Resposta 200 sem body de vídeo."),
            400 => throw new ValidationException(new[] { new ValidationFailure("", response.ErrorMessage ?? "ValidationFailed") }),
            404 => null,
            409 => throw new VideoUpdateConflictException(response.ErrorMessage ?? "Update conflict."),
            _ => throw new InvalidOperationException($"Lambda retornou status inesperado: {response.StatusCode} - {response.ErrorMessage}")
        };
    }
}
