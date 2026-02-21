using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using VideoProcessing.VideoManagement.Api.Filters;
using VideoProcessing.VideoManagement.Api.Models;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Api.Filters;

public class ApiResponseFilterTests
{
    private static ActionExecutedContext CreateContext(
        IActionResult? result,
        string path = "/videos",
        string? controllerName = "Videos")
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = path;
        var routeValues = new Dictionary<string, string?>();
        if (controllerName != null)
            routeValues["controller"] = controllerName;
        var actionDescriptor = new ActionDescriptor { RouteValues = routeValues };
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);
        var context = new ActionExecutedContext(actionContext, [], null!)
        {
            Result = result
        };
        return context;
    }

    [Fact]
    public void OnActionExecuted_OkObjectResult_WrapsInApiResponseWith200()
    {
        var payload = new { id = Guid.NewGuid(), title = "Test" };
        var context = CreateContext(new OkObjectResult(payload));
        var filter = new ApiResponseFilter();

        filter.OnActionExecuted(context);

        context.Result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)context.Result!;
        objectResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        objectResult.Value.Should().BeOfType<ApiResponse<object>>();
        var envelope = (ApiResponse<object>)objectResult.Value!;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void OnActionExecuted_ObjectResult201_WrapsInApiResponseWith201()
    {
        var payload = new { videoId = Guid.NewGuid() };
        var context = CreateContext(new ObjectResult(payload) { StatusCode = StatusCodes.Status201Created });
        var filter = new ApiResponseFilter();

        filter.OnActionExecuted(context);

        context.Result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)context.Result!;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        var envelope = (ApiResponse<object>)objectResult.Value!;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
    }

    [Fact]
    public void OnActionExecuted_NotFoundResult_DoesNotChange()
    {
        var original = new NotFoundResult();
        var context = CreateContext(original);
        var filter = new ApiResponseFilter();

        filter.OnActionExecuted(context);

        context.Result.Should().Be(original);
    }

    [Fact]
    public void OnActionExecuted_BadRequestResult_DoesNotChange()
    {
        var original = new BadRequestResult();
        var context = CreateContext(original);
        var filter = new ApiResponseFilter();

        filter.OnActionExecuted(context);

        context.Result.Should().Be(original);
    }

    [Fact]
    public void OnActionExecuted_HealthPath_DoesNotWrap()
    {
        var payload = new { status = "healthy" };
        var context = CreateContext(new OkObjectResult(payload), path: "/health", controllerName: "Health");
        var filter = new ApiResponseFilter();

        filter.OnActionExecuted(context);

        context.Result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)context.Result!;
        ok.Value.Should().BeEquivalentTo(payload);
    }

    [Fact]
    public void OnActionExecuted_ResultNull_DoesNotThrow()
    {
        var context = CreateContext(null);
        var filter = new ApiResponseFilter();

        var act = () => filter.OnActionExecuted(context);

        act.Should().NotThrow();
    }
}
