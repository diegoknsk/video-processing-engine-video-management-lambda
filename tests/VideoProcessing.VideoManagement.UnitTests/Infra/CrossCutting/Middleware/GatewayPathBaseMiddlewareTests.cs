using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using VideoProcessing.VideoManagement.Infra.CrossCutting.Middleware;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Infra.CrossCutting.Middleware;

[Collection("EnvironmentVariables")] // Prevent parallel execution conflicts
public class GatewayPathBaseMiddlewareTests : IDisposable
{
    private readonly Mock<ILogger<GatewayPathBaseMiddleware>> _loggerMock;

    public GatewayPathBaseMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<GatewayPathBaseMiddleware>>();
        // Ensure clean state
        Environment.SetEnvironmentVariable("GATEWAY_PATH_PREFIX", null);
        Environment.SetEnvironmentVariable("GATEWAY_STAGE", null);
    }

    public void Dispose()
    {
        // Cleanup
        Environment.SetEnvironmentVariable("GATEWAY_PATH_PREFIX", null);
        Environment.SetEnvironmentVariable("GATEWAY_STAGE", null);
    }

    [Fact]
    public async Task InvokeAsync_ShouldRemoveStage_And_MovePrefixToPathBase_WhenBothSet()
    {
        // Arrange
        Environment.SetEnvironmentVariable("GATEWAY_STAGE", "default");
        Environment.SetEnvironmentVariable("GATEWAY_PATH_PREFIX", "/videos");

        var context = new DefaultHttpContext();
        context.Request.Path = "/default/videos/health";
        context.Request.PathBase = "";

        var middleware = new GatewayPathBaseMiddleware(
            next: (innerHttpContext) =>
            {
                // Assert inside the pipeline
                innerHttpContext.Request.Path.Value.Should().Be("/health");
                innerHttpContext.Request.PathBase.Value.Should().Be("/videos");
                return Task.CompletedTask;
            },
            logger: _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Assertions are done inside the next delegate to verify state DURING pipeline execution
    }

    [Fact]
    public async Task InvokeAsync_ShouldRemoveStage_WhenOnlyStageSet()
    {
        // Arrange
        Environment.SetEnvironmentVariable("GATEWAY_STAGE", "dev");
        Environment.SetEnvironmentVariable("GATEWAY_PATH_PREFIX", null);

        var context = new DefaultHttpContext();
        context.Request.Path = "/dev/health";

        var middleware = new GatewayPathBaseMiddleware(
            next: (innerHttpContext) =>
            {
                innerHttpContext.Request.Path.Value.Should().Be("/health");
                innerHttpContext.Request.PathBase.Value.Should().Be("");
                return Task.CompletedTask;
            },
            logger: _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(context);
    }

    [Fact]
    public async Task InvokeAsync_ShouldMovePrefixToPathBase_WhenOnlyPrefixSet()
    {
        // Arrange
        Environment.SetEnvironmentVariable("GATEWAY_STAGE", null);
        Environment.SetEnvironmentVariable("GATEWAY_PATH_PREFIX", "/api");

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/users";

        var middleware = new GatewayPathBaseMiddleware(
            next: (innerHttpContext) =>
            {
                innerHttpContext.Request.Path.Value.Should().Be("/users");
                innerHttpContext.Request.PathBase.Value.Should().Be("/api");
                return Task.CompletedTask;
            },
            logger: _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(context);
    }

    [Fact]
    public async Task InvokeAsync_ShouldDoNothing_WhenVariablesNotSet()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/health";

        var middleware = new GatewayPathBaseMiddleware(
            next: (innerHttpContext) =>
            {
                innerHttpContext.Request.Path.Value.Should().Be("/health");
                return Task.CompletedTask;
            },
            logger: _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(context);
    }
    
    [Theory]
    [InlineData("/default/videos/health", "default", "/videos", "/videos", "/health")]
    [InlineData("/DEFAULT/VIDEOS/health", "default", "/videos", "/videos", "/health")] // Case insensitive
    public async Task InvokeAsync_ShouldHandleCaseInsensitivity(string requestPath, string stage, string prefix, string expectedPathBase, string expectedPath)
    {
        // Arrange
        Environment.SetEnvironmentVariable("GATEWAY_STAGE", stage);
        Environment.SetEnvironmentVariable("GATEWAY_PATH_PREFIX", prefix);

        var context = new DefaultHttpContext();
        context.Request.Path = requestPath;

        var middleware = new GatewayPathBaseMiddleware(
            next: (innerHttpContext) =>
            {
                innerHttpContext.Request.Path.Value.Should().BeEquivalentTo(expectedPath);
                innerHttpContext.Request.PathBase.Value.Should().BeEquivalentTo(expectedPathBase);
                return Task.CompletedTask;
            },
            logger: _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(context);
    }
}
