using Amazon.DynamoDBv2;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;

namespace VideoProcessing.VideoManagement.Infra.Data.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureData(this IServiceCollection services)
    {
        // Register AWS Clients
        services.AddSingleton<IAmazonDynamoDB>(sp => new AmazonDynamoDBClient());
        services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client());

        // Register Repositories (Placeholders for now)
        // services.AddScoped<IVideoRepository, VideoRepository>();

        return services;
    }
}
