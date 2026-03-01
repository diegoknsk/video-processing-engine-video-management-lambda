using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VideoProcessing.VideoManagement.Api.Filters;

/// <summary>
/// Preenche o Server do OpenAPI para o "Try it" do Scalar. Quando GATEWAY_STAGE/GATEWAY_PATH_PREFIX estão definidos,
/// monta o path do server só a partir da configuração (evita duplicar stage); senão usa PathBase do request (local).
/// API_PUBLIC_BASE_URL tem prioridade quando definida.
/// </summary>
public sealed class OpenApiServerFromRequestFilter : IDocumentFilter
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public OpenApiServerFromRequestFilter(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        document.Servers ??= [];

        var apiPublicBaseUrl = _configuration["API_PUBLIC_BASE_URL"];
        if (!string.IsNullOrWhiteSpace(apiPublicBaseUrl))
        {
            var url = apiPublicBaseUrl.TrimEnd('/');
            if (!document.Servers.Any(s => s.Url == url))
                document.Servers.Insert(0, new OpenApiServer { Url = url, Description = "API (API_PUBLIC_BASE_URL)" });
            return;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request != null)
        {
            var request = httpContext.Request;
            var scheme = request.Scheme;
            var host = request.Host.Value;
            var stage = _configuration["GATEWAY_STAGE"]?.Trim();
            var pathPrefix = _configuration["GATEWAY_PATH_PREFIX"]?.Trim();
            var hasGatewayConfig = !string.IsNullOrWhiteSpace(stage) || !string.IsNullOrWhiteSpace(pathPrefix);

            var pathSegment = hasGatewayConfig
                ? BuildServerPathFromConfig(stage, pathPrefix)
                : (request.PathBase.Value ?? "");

            var serverUrl = $"{scheme}://{host}{pathSegment}".TrimEnd('/');
            if (!string.IsNullOrEmpty(serverUrl) && !document.Servers.Any(s => s.Url == serverUrl))
                document.Servers.Insert(0, new OpenApiServer { Url = serverUrl, Description = "Current request" });
        }
    }

    private static string BuildServerPathFromConfig(string? stage, string? pathPrefix)
    {
        var stagePart = string.IsNullOrWhiteSpace(stage) ? "" : $"/{stage!.Trim()}";
        var prefixPart = string.IsNullOrWhiteSpace(pathPrefix) ? "" : pathPrefix!.Trim().StartsWith('/') ? pathPrefix.Trim() : $"/{pathPrefix.Trim()}";
        return $"{stagePart}{prefixPart}";
    }
}
