using System.Text.Json;
using Microsoft.AspNetCore.Http;
using VideoProcessing.VideoManagement.Api.Models;

namespace VideoProcessing.VideoManagement.Api.Middleware;

/// <summary>
/// Captura exceções não tratadas e retorna resposta JSON no formato ApiErrorResponse.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {ExceptionType}", ex.GetType().Name);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, code, message) = ExceptionMapper.Map(exception);
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var errorResponse = ApiErrorResponse.Create(code, message);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
}
