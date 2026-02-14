using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VideoProcessing.VideoManagement.Application.Models;

namespace VideoProcessing.VideoManagement.Infra.CrossCutting.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            
            _logger.LogError(ex, "An unhandled exception has occurred. TraceId: {TraceId}", traceId);

            var response = new ErrorResponse(
                Type: "https://tools.ietf.org/html/rfc7807",
                Title: "Internal Server Error",
                Status: StatusCodes.Status500InternalServerError,
                Detail: _env.IsDevelopment() ? ex.Message : "An error occurred while processing your request.",
                TraceId: traceId
            );

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
