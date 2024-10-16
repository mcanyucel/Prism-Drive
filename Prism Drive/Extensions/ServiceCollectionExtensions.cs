using CommunityToolkit.Maui;
using Prism_Drive.Services;
using Prism_Drive.Services.Implementation;
using Prism_Drive.ViewModels;

namespace Prism_Drive.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        return services;
    }

    public static IServiceCollection AddPrismServices(this IServiceCollection services)
    {
        services.AddSingleton<IPrismService, PrismServicev2>();
        return services;
    }

    public static IServiceCollection AddUserServices(this IServiceCollection services)
    {
        services.AddSingleton<IUserService, UserService>();
        return services;
    }

    public static IServiceCollection AddViewModelMappings(this IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();

        return services;
    }
}
