using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VideoProcessing.VideoManagement.Api.Filters;

/// <summary>
/// Aplica o requisito de segurança BearerAuth apenas em operações que exigem autenticação
/// (exclui rotas públicas como GET /health).
/// </summary>
public sealed class BearerAuthSecurityOperationFilter : IOperationFilter
{
    private const string BearerAuthSchemeId = "BearerAuth";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true)
            .OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>()
            .Any();
        if (hasAllowAnonymous)
            return;

        var hasAuthorize = context.MethodInfo.GetCustomAttributes(true)
            .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .Any();
        if (!hasAuthorize)
            return;

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = BearerAuthSchemeId
                }
            }] = []
        });
    }
}
