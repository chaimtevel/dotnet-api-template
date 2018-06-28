using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Auth;
using Services.DbContexts;
using Shared.Auth;

namespace Services.Extensions
{
    public static class DependencyInjection
    {

        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddJwtService()
                .AddAuthService()
                .AddEFCoreIdentity(configuration)
                .AddAutoMapper(e =>
                {
                    e.CreateMap<IdentityUser, ApiUser>(MemberList.None);
                });

            return services;
        }

        public static IServiceCollection AddAuthService(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            return services;
        }

        public static IServiceCollection AddJwtService(this IServiceCollection services)
        {
            services.AddScoped<IJwtService, JwtService>();

            return services;
        }

        public static IServiceCollection AddEFCoreIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            // modify for the appropriate db engine
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }

    }
}
