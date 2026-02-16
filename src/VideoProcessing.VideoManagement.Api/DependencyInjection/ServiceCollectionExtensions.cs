using Amazon.DynamoDBv2;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VideoProcessing.VideoManagement.Application.Ports;
using VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;
using VideoProcessing.VideoManagement.Infra.Data.Repositories;

namespace VideoProcessing.VideoManagement.Api.DependencyInjection;

/// <summary>
/// Composition root: registra todos os serviços da aplicação (Clean Architecture — wiring na API).
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVideoManagementServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Cross-cutting: Options
        services.AddOptions<AwsOptions>()
            .Bind(configuration.GetSection("AWS"));

        services.AddOptions<DynamoDbOptions>()
            .Bind(configuration.GetSection("DynamoDB"));

        services.AddOptions<S3Options>()
            .Bind(configuration.GetSection("S3"));

        services.AddOptions<CognitoOptions>()
            .Bind(configuration.GetSection("Cognito"));

        // Infra.Data: clientes AWS e repositórios
        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient());
        services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client());
        services.AddScoped<IVideoRepository, VideoRepository>();

        // Application: UseCases e validators (quando existirem)
        // services.AddScoped<ICreateVideoUseCase, CreateVideoUseCase>();

        return services;
    }
}
