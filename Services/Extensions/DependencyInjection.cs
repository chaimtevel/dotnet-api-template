using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Auth;

namespace Services.Extensions
{
    public static class DependencyInjection
    {

        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddJwtService()
                .AddEFCoreIdentity(configuration);

            return services;
        }

        public static IServiceCollection AddJwtService(this IServiceCollection services)
        {
            services.AddTransient<IJwtService, JwtService>();

            return services;
        }

        public static IServiceCollection AddEFCoreIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            // modify for the appropriate db engine
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>();

            return services;
        }

    }
}
