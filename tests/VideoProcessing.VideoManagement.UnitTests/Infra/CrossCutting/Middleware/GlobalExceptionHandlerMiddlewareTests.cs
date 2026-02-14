using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using VideoProcessing.VideoManagement.Application.Models;
using VideoProcessing.VideoManagement.Infra.CrossCutting.Middleware;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Infra.CrossCutting.Middleware;

public class GlobalExceptionHandlerMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldReturn500_WhenExceptionOccurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new GlobalExceptionHandlerMiddleware(
            next: (innerHttpContext) => throw new Exception("Test exception"),
            logger: loggerMock.Object,
            env: envMock.Object
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        errorResponse.Should().NotBeNull();
        errorResponse!.Status.Should().Be(500);
        errorResponse.Detail.Should().Be("Test exception"); // In Development
    }
}
