using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace VideoProcessing.VideoManagement.Infra.CrossCutting.Middleware;

public class GatewayPathBaseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GatewayPathBaseMiddleware> _logger;

    public GatewayPathBaseMiddleware(RequestDelegate next, ILogger<GatewayPathBaseMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var pathPrefix = Environment.GetEnvironmentVariable("GATEWAY_PATH_PREFIX");
        var stage = Environment.GetEnvironmentVariable("GATEWAY_STAGE");
        
        var originalPath = context.Request.Path;
        var originalPathBase = context.Request.PathBase;
        var inputPath = originalPath;

        // 1. Remove Stage if present
        if (!string.IsNullOrEmpty(stage))
        {
            var stageSegment = $"/{stage}";
            if (context.Request.Path.StartsWithSegments(stageSegment, StringComparison.OrdinalIgnoreCase, out var remainingPathAfterStage))
            {
                context.Request.Path = remainingPathAfterStage;
                _logger.LogDebug("Removed Gateway Stage '{Stage}' from path. Path changed from '{Original}' to '{NewPath}'", 
                    stage, originalPath, context.Request.Path);
            }
        }

        // 2. Handle Path Prefix
        if (!string.IsNullOrEmpty(pathPrefix))
        {
             // Add leading slash if missing for comparison
             if (!pathPrefix.StartsWith("/")) pathPrefix = "/" + pathPrefix;

             if (context.Request.Path.StartsWithSegments(pathPrefix, StringComparison.OrdinalIgnoreCase, out var remainingPathAfterPrefix))
             {
                 // Move prefix to PathBase
                 context.Request.PathBase = context.Request.PathBase.Add(pathPrefix);
                 context.Request.Path = remainingPathAfterPrefix;
                 
                 _logger.LogDebug("Moved Gateway Prefix '{Prefix}' to PathBase. PathBase: '{PathBase}', Path: '{Path}'", 
                     pathPrefix, context.Request.PathBase, context.Request.Path);
             }
        }

        await _next(context);
    }
}
