using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VideoProcessing.VideoManagement.Api.Models;

namespace VideoProcessing.VideoManagement.Api.Filters;

/// <summary>
/// Encapsula respostas 200/201 em ApiResponse&lt;T&gt; (success, data, timestamp).
/// Exclui /health para manter corpo esperado por load balancers/ferramentas.
/// </summary>
public class ApiResponseFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is null)
            return;

        if (ShouldSkipEnvelope(context))
            return;

        if (context.Result is OkObjectResult okResult)
        {
            context.Result = new ObjectResult(ApiResponse<object>.CreateSuccess(okResult.Value!))
                { StatusCode = StatusCodes.Status200OK };
            return;
        }

        if (context.Result is ObjectResult objectResult &&
            (objectResult.StatusCode == StatusCodes.Status200OK || objectResult.StatusCode == StatusCodes.Status201Created))
        {
            context.Result = new ObjectResult(ApiResponse<object>.CreateSuccess(objectResult.Value!))
                { StatusCode = objectResult.StatusCode };
        }
    }

    private static bool ShouldSkipEnvelope(ActionExecutedContext context)
    {
        var path = context.HttpContext.Request.Path;
        if (path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase))
            return true;

        if (context.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller) &&
            string.Equals(controller, "Health", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
