using Microsoft.Extensions.DependencyInjection;

namespace VideoProcessing.VideoManagement.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register UseCases (Placeholders for now)
        // services.AddScoped<ICreateVideoUseCase, CreateVideoUseCase>();

        return services;
    }
}
