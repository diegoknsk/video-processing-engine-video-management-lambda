using Amazon.DynamoDBv2;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VideoProcessing.VideoManagement.Application.Ports;
using VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;
using VideoProcessing.VideoManagement.Application.Configuration;
using VideoProcessing.VideoManagement.Infra.Data.Repositories;
using VideoProcessing.VideoManagement.Infra.Data.Services;
using FluentValidation;
using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.UseCases.GetVideoById;
using VideoProcessing.VideoManagement.Application.UseCases.ListVideos;
using VideoProcessing.VideoManagement.Application.UseCases.UploadVideo;
using VideoProcessing.VideoManagement.Application.Validators;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

        // Authentication: JWT Bearer (Cognito)
        var cognitoSection = configuration.GetSection("Cognito");
        var cognitoRegion = cognitoSection["Region"] ?? "us-east-1";
        var cognitoUserPoolId = cognitoSection["UserPoolId"] ?? string.Empty;
        var cognitoClientId = cognitoSection["ClientId"] ?? string.Empty;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"https://cognito-idp.{cognitoRegion}.amazonaws.com/{cognitoUserPoolId}";
                options.MapInboundClaims = false; // garante que "sub" chegue como "sub"
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false, // Cognito access tokens não têm aud = clientId
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    NameClaimType = "sub" // User.Identity.Name retorna o sub
                };
            });

        // Infra.Data: clientes AWS e repositórios
        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient());
        services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client());
        services.AddScoped<IVideoRepository, VideoRepository>();

        // Infra.Data: Services
        services.AddScoped<IS3PresignedUrlService, S3PresignedUrlService>();

        // Validators
        services.AddScoped<IValidator<UploadVideoInputModel>, UploadVideoInputModelValidator>();
        
        // Application: UseCases
        services.AddScoped<IUploadVideoUseCase, UploadVideoUseCase>();
        services.AddScoped<IListVideosUseCase, ListVideosUseCase>();
        services.AddScoped<IGetVideoByIdUseCase, GetVideoByIdUseCase>();

        return services;
    }
}
