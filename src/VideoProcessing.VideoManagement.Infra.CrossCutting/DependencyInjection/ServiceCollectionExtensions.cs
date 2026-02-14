using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;

namespace VideoProcessing.VideoManagement.Infra.CrossCutting.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCrossCuttingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AwsOptions>()
            .Bind(configuration.GetSection("AWS"));
            // .ValidateDataAnnotations()
            // .ValidateOnStart();

        services.AddOptions<DynamoDbOptions>()
            .Bind(configuration.GetSection("DynamoDB"));
            // .ValidateDataAnnotations()
            // .ValidateOnStart();

        services.AddOptions<S3Options>()
            .Bind(configuration.GetSection("S3"));
            // .ValidateDataAnnotations()
            // .ValidateOnStart();

        services.AddOptions<CognitoOptions>()
            .Bind(configuration.GetSection("Cognito"));
            // .ValidateDataAnnotations()
            // .ValidateOnStart();

        return services;
    }
}
