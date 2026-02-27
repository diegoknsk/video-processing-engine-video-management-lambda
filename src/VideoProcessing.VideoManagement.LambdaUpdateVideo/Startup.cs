using Amazon.DynamoDBv2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VideoProcessing.VideoManagement.Application.Ports;
using VideoProcessing.VideoManagement.Application.UseCases.UpdateVideo;
using VideoProcessing.VideoManagement.Application.Validators;
using VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;
using VideoProcessing.VideoManagement.Infra.Data.Repositories;

namespace VideoProcessing.VideoManagement.LambdaUpdateVideo;

/// <summary>
/// Bootstrap da Lambda: configura DynamoDB, registra Use Case (Application) e handler (borda). Sem lógica duplicada.
/// Variáveis de ambiente: DynamoDB__TableName, DynamoDB__Region (ou AWS__Region).
/// </summary>
public static class Startup
{
    public static IServiceProvider BuildServiceProvider()
    {
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConfiguration(config.GetSection("Logging"));
            builder.AddConsole();
        });

        services.Configure<DynamoDbOptions>(config.GetSection("DynamoDB"));
        var region = config["DynamoDB:Region"] ?? config["AWS:Region"] ?? Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(Amazon.RegionEndpoint.GetBySystemName(region)));
        services.AddSingleton<IVideoRepository, VideoRepository>();

        services.AddSingleton<FluentValidation.IValidator<VideoProcessing.VideoManagement.Application.Models.InputModels.UpdateVideoInputModel>, UpdateVideoInputModelValidator>();
        services.AddSingleton<IUpdateVideoUseCase, UpdateVideoUseCase>();
        services.AddSingleton<IUpdateVideoHandler, UpdateVideoHandler>();

        return services.BuildServiceProvider();
    }
}
