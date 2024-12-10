using Bot_Application.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot_Application;

public static class ServiceExtension
{
    public static void AddApplicationLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ICreateDynamicCommands, CreateDynamicCommands>();

    }
}