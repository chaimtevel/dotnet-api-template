using Microsoft.Extensions.DependencyInjection;
using Services.Auth;

namespace Services.Extensions
{
    public static class DependencyInjection
    {

        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services
                .AddJwtService();

            return services;
        }

        public static IServiceCollection AddJwtService(this IServiceCollection services)
        {
            services.AddTransient<IJwtService, JwtService>();

            return services;
        }

    }
}
