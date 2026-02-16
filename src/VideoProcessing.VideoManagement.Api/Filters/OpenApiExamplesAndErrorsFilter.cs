using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VideoProcessing.VideoManagement.Api.Filters;

/// <summary>
/// Adiciona exemplos de request/response e descrições padronizadas para respostas de erro no OpenAPI.
/// </summary>
public sealed class OpenApiExamplesAndErrorsFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        SetErrorResponseDescriptions(operation);

        var method = context.ApiDescription.HttpMethod;
        var relativePath = context.ApiDescription.RelativePath ?? "";

        if (method == "POST" && relativePath.Equals("videos", StringComparison.OrdinalIgnoreCase))
            AddUploadVideoExamples(operation, context);
        else if (method == "GET" && relativePath.StartsWith("videos/", StringComparison.OrdinalIgnoreCase) && relativePath.Length > 7)
            AddGetVideoResponseExample(operation);
    }

    private static void SetErrorResponseDescriptions(OpenApiOperation operation)
    {
        var descriptions = new Dictionary<string, string>
        {
            ["400"] = "Bad Request: dados de entrada inválidos ou validação falhou.",
            ["401"] = "Unauthorized: token JWT ausente, inválido ou expirado.",
            ["404"] = "Not Found: recurso não encontrado.",
            ["409"] = "Conflict: conflito de estado ou idempotência (clientRequestId duplicado).",
            ["500"] = "Internal Server Error: erro interno do servidor."
        };
        foreach (var (code, desc) in descriptions)
        {
            if (operation.Responses.TryGetValue(code, out var response))
                response.Description = desc;
        }
    }

    private static void AddUploadVideoExamples(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody?.Content.TryGetValue("application/json", out var requestMedia) == true)
        {
            requestMedia.Example = new OpenApiObject
            {
                ["originalFileName"] = new OpenApiString("meu-video.mp4"),
                ["contentType"] = new OpenApiString("video/mp4"),
                ["sizeBytes"] = new OpenApiInteger(1024 * 1024 * 50),
                ["durationSec"] = new OpenApiDouble(120.5),
                ["clientRequestId"] = new OpenApiString("req-abc-123")
            };
        }

        if (operation.Responses.TryGetValue("200", out var okResponse) && okResponse.Content.TryGetValue("application/json", out var okMedia))
        {
            okMedia.Example = new OpenApiObject
            {
                ["videoId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                ["uploadUrl"] = new OpenApiString("https://s3.amazonaws.com/bucket/key?X-Amz-Signature=..."),
                ["expiresAt"] = new OpenApiString(DateTime.UtcNow.AddMinutes(15).ToString("O"))
            };
        }
    }

    private static void AddGetVideoResponseExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var okResponse) && okResponse.Content.TryGetValue("application/json", out var okMedia))
        {
            okMedia.Example = new OpenApiObject
            {
                ["videoId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                ["userId"] = new OpenApiString("7ba85f64-5717-4562-b3fc-2c963f66afa6"),
                ["originalFileName"] = new OpenApiString("meu-video.mp4"),
                ["contentType"] = new OpenApiString("video/mp4"),
                ["sizeBytes"] = new OpenApiLong(52428800),
                ["durationSec"] = new OpenApiDouble(120.5),
                ["status"] = new OpenApiString("Processing"),
                ["processingMode"] = new OpenApiString("SingleLambda"),
                ["progressPercent"] = new OpenApiInteger(45),
                ["createdAt"] = new OpenApiString(DateTime.UtcNow.AddHours(-1).ToString("O")),
                ["updatedAt"] = new OpenApiString(DateTime.UtcNow.ToString("O"))
            };
        }
    }
}
