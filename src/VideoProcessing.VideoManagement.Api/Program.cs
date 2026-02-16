
using VideoProcessing.VideoManagement.Infra.CrossCutting.Middleware;
using Serilog;
using Serilog.Formatting.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.AspNetCore.Http;
using Amazon.Lambda.AspNetCoreServer.Hosting;
using Scalar.AspNetCore;
using VideoProcessing.VideoManagement.Api.Extensions;
using VideoProcessing.VideoManagement.Api.DependencyInjection;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new JsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Add Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(new JsonFormatter()));

    builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

    // Add Configuration
    builder.Configuration.AddEnvironmentVariables();

    // Add Services (DI — composition root na API)
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddVideoManagementServices(builder.Configuration);
    builder.Services.AddOpenApiDocumentation();
    builder.Services.AddControllers();

    var app = builder.Build();

    // Pipeline
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
            diagnosticContext.Set("UserId", httpContext.User?.FindFirst("sub")?.Value);
        };
    });
    
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    app.UseMiddleware<GatewayPathBaseMiddleware>();
    app.UseRouting();

    app.UseOpenApiDocumentation();

    // Documentação interativa: acesse https://localhost:7163/scalar (ou /scalar na sua URL base)
    app.MapScalarApiReference("/scalar", options => options
        .WithTitle("Video Management API")
        .AddDocument("v1", "Video Management API v1", "/swagger/v1/swagger.json"));

    app.MapControllers();

    // Root Redirect (optional helpful default)
    app.MapGet("/", () => Results.Redirect("/health"));

    // Alias para documentação OpenAPI (story: GET /openapi/v1.json)
    app.MapGet("/openapi/v1.json", (HttpContext ctx) =>
        Results.Redirect(ctx.Request.PathBase + "/swagger/v1/swagger.json", permanent: false));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
