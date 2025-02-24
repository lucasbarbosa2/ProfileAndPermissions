namespace ProfileAndPermissions.UI
{
    public static class ServiceColletion
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            Application.DependecyInjection.AddServices(services);

            return services;
        }
    }
}
