using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VideoProcessing.VideoManagement.Api.Filters;

/// <summary>
/// Preenche o Server do OpenAPI a partir do request (Scheme + Host + PathBase) ou de API_PUBLIC_BASE_URL,
/// para que o "Try it" do Scalar use a URL correta (incluindo atrás do API Gateway).
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
            var pathBase = request.PathBase.Value ?? "";
            var stage = _configuration["GATEWAY_STAGE"];
            var pathBaseWithStage = string.IsNullOrWhiteSpace(stage)
                ? pathBase
                : $"/{stage.Trim()}{pathBase}";
            var serverUrl = $"{scheme}://{host}{pathBaseWithStage}".TrimEnd('/');
            if (!string.IsNullOrEmpty(serverUrl) && !document.Servers.Any(s => s.Url == serverUrl))
                document.Servers.Insert(0, new OpenApiServer { Url = serverUrl, Description = "Current request" });
        }
    }
}
