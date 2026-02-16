using System.Reflection;
using Microsoft.OpenApi.Models;

namespace VideoProcessing.VideoManagement.Api.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Video Management API",
                Version = "1.0.0",
                Description = "API para gerenciamento de vídeos: registro, upload via presigned URL, listagem e atualização de status. Autenticação via Amazon Cognito (JWT).",
                Contact = new OpenApiContact
                {
                    Name = "Video Processing",
                    Url = new Uri("https://github.com/fiap/video-processing-engine")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            var apiXml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            if (File.Exists(apiXml))
                options.IncludeXmlComments(apiXml);

            var applicationXml = Path.Combine(AppContext.BaseDirectory, "VideoProcessing.VideoManagement.Application.xml");
            if (File.Exists(applicationXml))
                options.IncludeXmlComments(applicationXml);

            options.AddSecurityDefinition("BearerAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT obtido do Amazon Cognito User Pool (claim 'sub' = userId)."
            });
            options.OperationFilter<Filters.BearerAuthSecurityOperationFilter>();
            options.OperationFilter<Filters.OpenApiExamplesAndErrorsFilter>();
            options.DocumentFilter<Filters.OpenApiServerFromRequestFilter>();
        });

        return services;
    }

    public static IApplicationBuilder UseOpenApiDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger(options => options.RouteTemplate = "swagger/{documentName}/swagger.json");
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Video Management API v1");
            options.RoutePrefix = "swagger";
        });
        return app;
    }
}
