using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Auth;
using Services.DbContexts;
using Services.HelperServices;
using Shared.Models.DB.Identity;

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
                .AddScoped(typeof(IGenericDataService<>), typeof(GenericDataService<>))
                .AddAutoMapper(e =>
                {
                    
                }, typeof(DependencyInjection).Assembly);

            return services;
        }

        public static IServiceCollection AddAuthService(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>()
                .AddScoped<IAccountServices, AccountServices>();

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
            services.AddDbContextPool<ApplicationDbContext>(options => options
                .UseSqlServer(configuration.GetConnectionString("DefaultConnection"), o => o.EnableRetryOnFailure())
                .UseLazyLoadingProxies());

            services.AddIdentity<User, Role>(o =>
            {
                o.Password.RequiredLength = 8;
            })
            .AddUserManager<AccountServices>()
            .AddUserStore<UserStore>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            return services;
        }

    }
}
