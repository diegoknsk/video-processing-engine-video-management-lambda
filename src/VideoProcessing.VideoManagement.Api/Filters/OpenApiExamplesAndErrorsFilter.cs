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
        {
            AddUploadVideoDocumentation(operation);
            AddUploadVideoExamples(operation, context);
        }
        else if (method == "GET" && relativePath.StartsWith("videos/", StringComparison.OrdinalIgnoreCase) && relativePath.Length > 7)
            AddGetVideoResponseExample(operation);
        else if (method == "PATCH" && relativePath.StartsWith("videos/", StringComparison.OrdinalIgnoreCase))
            operation.Summary = "Internal route for orchestrator/processor/finalizer. Atualização parcial (status, progresso, erros, S3).";
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

    private static void AddUploadVideoDocumentation(OpenApiOperation operation)
    {
        operation.Description =
            "Registra um novo vídeo e retorna URL pré-assinada para upload no S3. **ClientRequestId:** opcional. " +
            "Se informado, o mesmo valor para o mesmo usuário retorna o mesmo videoId (idempotência para retries). " +
            "Para **múltiplos vídeos**, envie um UUID diferente por arquivo ou omita o campo; não use valor fixo (ex.: userId) em todos os POSTs.";
    }

    private static void AddUploadVideoExamples(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody?.Content.TryGetValue("application/json", out var requestMedia) == true)
        {
            requestMedia.Example = new OpenApiObject
            {
                ["originalFileName"] = new OpenApiString("meu-video.mp4"),
                ["contentType"] = new OpenApiString("video/mp4"),
                ["sizeKb"] = new OpenApiLong(51200),
                ["durationSec"] = new OpenApiDouble(120.5),
                ["clientRequestId"] = new OpenApiString("a1b2c3d4-e5f6-4789-a012-3456789abcde")
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
