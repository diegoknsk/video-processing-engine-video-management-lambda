using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using VideoProcessing.VideoManagement.Api.Filters;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Api.Filters;

/// <summary>
/// Controllers auxiliares para testar o BearerAuthSecurityOperationFilter.
/// </summary>
[Authorize]
internal sealed class TestAuthorizeController
{
    public IActionResult Get() => new OkResult();
}

[AllowAnonymous]
internal sealed class TestAllowAnonymousController
{
    public IActionResult Get() => new OkResult();
}

internal sealed class TestNoAuthorizeController
{
    public IActionResult Get() => new OkResult();
}

public class BearerAuthSecurityOperationFilterTests
{
    private static OperationFilterContext CreateContext(MethodInfo methodInfo)
    {
        var descriptor = new ControllerActionDescriptor
        {
            MethodInfo = methodInfo,
            ControllerTypeInfo = methodInfo.DeclaringType!.GetTypeInfo()
        };
        var apiDescription = new ApiDescription
        {
            ActionDescriptor = descriptor
        };
        var schemaRepository = new SchemaRepository();
        var schemaGenerator = new Mock<ISchemaGenerator>().Object;

        return new OperationFilterContext(apiDescription, schemaGenerator, schemaRepository, methodInfo);
    }

    [Fact]
    public void Apply_WhenMethodHasClassLevelAuthorize_AddsBearerSecurity()
    {
        // Arrange
        var methodInfo = typeof(TestAuthorizeController).GetMethod(nameof(TestAuthorizeController.Get))!;
        var context = CreateContext(methodInfo);
        var operation = new OpenApiOperation();
        var filter = new BearerAuthSecurityOperationFilter();

        // Act
        filter.Apply(operation, context);

        // Assert
        operation.Security.Should().NotBeEmpty();
        operation.Security.Should().ContainSingle();
        var scheme = operation.Security[0].Keys.Single();
        scheme.Reference!.Id.Should().Be("BearerAuth");
    }

    [Fact]
    public void Apply_WhenMethodHasAllowAnonymous_DoesNotAddSecurity()
    {
        // Arrange
        var methodInfo = typeof(TestAllowAnonymousController).GetMethod(nameof(TestAllowAnonymousController.Get))!;
        var context = CreateContext(methodInfo);
        var operation = new OpenApiOperation();
        var filter = new BearerAuthSecurityOperationFilter();

        // Act
        filter.Apply(operation, context);

        // Assert
        operation.Security.Should().BeEmpty();
    }

    [Fact]
    public void Apply_WhenMethodHasNoAuthorize_DoesNotAddSecurity()
    {
        // Arrange
        var methodInfo = typeof(TestNoAuthorizeController).GetMethod(nameof(TestNoAuthorizeController.Get))!;
        var context = CreateContext(methodInfo);
        var operation = new OpenApiOperation();
        var filter = new BearerAuthSecurityOperationFilter();

        // Act
        filter.Apply(operation, context);

        // Assert
        operation.Security.Should().BeEmpty();
    }
}
