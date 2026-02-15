
using VideoProcessing.VideoManagement.Infra.CrossCutting.Middleware;
using VideoProcessing.VideoManagement.Infra.CrossCutting.DependencyInjection;
using VideoProcessing.VideoManagement.Infra.Data.DependencyInjection;
using Serilog;
using Serilog.Formatting.Json;
using VideoProcessing.VideoManagement.Application.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.AspNetCore.Http; // For Results
using Amazon.Lambda.AspNetCoreServer.Hosting;

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

    // Add Services (DI)
    builder.Services.AddCrossCuttingConfiguration(builder.Configuration);
    builder.Services.AddInfrastructureData();
    builder.Services.AddApplicationServices();
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

    // Map Controllers
    app.MapControllers();

    // Root Redirect (optional helpful default)
    app.MapGet("/", () => Results.Redirect("/health"));

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
