using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using VideoProcessing.VideoManagement.Api.Middleware;
using VideoProcessing.VideoManagement.Api.Models;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Api.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private static async Task<string> GetResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return await new StreamReader(context.Response.Body).ReadToEndAsync();
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ReturnsApiErrorResponseWith500()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new GlobalExceptionMiddleware(
            _ => throw new Exception("Unexpected"),
            loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/json");
        var body = await GetResponseBody(context);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var error = JsonSerializer.Deserialize<ApiErrorResponse>(body, options);
        error.Should().NotBeNull();
        error!.Success.Should().BeFalse();
        error.Error.Code.Should().Be("InternalServerError");
        error.Error.Message.Should().Be("Erro interno do servidor.");
        error.Timestamp.Should().NotBe(default);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedAccessException_Returns401()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new GlobalExceptionMiddleware(
            _ => throw new UnauthorizedAccessException(),
            loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        var body = await GetResponseBody(context);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var error = JsonSerializer.Deserialize<ApiErrorResponse>(body, options);
        error!.Error.Code.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task InvokeAsync_WhenKeyNotFoundException_Returns404()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new GlobalExceptionMiddleware(
            _ => throw new KeyNotFoundException(),
            loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var body = await GetResponseBody(context);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var error = JsonSerializer.Deserialize<ApiErrorResponse>(body, options);
        error!.Error.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_InvokesNext()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var context = new DefaultHttpContext();
        var nextCalled = false;
        var middleware = new GlobalExceptionMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            loggerMock.Object);

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }
}
