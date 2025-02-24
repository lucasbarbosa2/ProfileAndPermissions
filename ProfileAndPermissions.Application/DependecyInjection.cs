using Microsoft.Extensions.DependencyInjection;
using ProfileAndPermissions.Application.Background;
using ProfileAndPermissions.Application.Services;
using ProfileAndPermissions.Domain.Interfaces;

namespace ProfileAndPermissions.Application
{
    public static class DependecyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IProfileConfigurationService, ProfileConfigurationService>();
            services.AddHostedService<ToggleConfigurationService>();
            return services;
        }
    }
}
