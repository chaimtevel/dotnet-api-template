using FastExpressionCompiler;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.AppServices;
using Services.Auth;
using Services.EF;
using Services.HelperServices;
using Shared.Mappings;
using Shared.Models.DB.Identity;
using Shared.Options;
using System.Reflection;

namespace Services.Extensions;

public static class DependencyInjection
{

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, Assembly apiAssembly)
    {
        services
            .AddJwtService()
            .AddAuthService()
            .AddDb(configuration)
            .AddEFCoreIdentity()
            .AddHttpContextAccessor()
            .AddMessagingService(configuration)
            .AddMapster(new[] { apiAssembly, typeof(MapsterGenericRegister).Assembly })
            .AddDataProtection()
            .AddScoped(typeof(IGenericDataService<>), typeof(GenericDataService<>))
            .AddScoped<IPersonService, PersonService>()
            .AddScoped<IGeoService, GeoService>()
            .AddSingleton<ICachingService, CachingService>();

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

    public static IServiceCollection AddDb(this IServiceCollection services, IConfiguration configuration, bool doPool = true)
    {
        // modify for the appropriate db engine
        // note that pooled connections can result in memory leaks
        string connectionString = configuration.GetConnectionString("DefaultConnection");
        void optionsConfig(DbContextOptionsBuilder o) => o
            .UseSqlServer(connectionString, o => o.EnableRetryOnFailure().UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
            .UseLazyLoadingProxies();

        return doPool ? services.AddDbContextPool<AppDbContext>(optionsConfig) : services.AddDbContext<AppDbContext>(optionsConfig);
    }

    public static IServiceCollection AddEFCoreIdentity(this IServiceCollection services)
    {
        services.AddIdentity<User, Role>(o =>
        {
            o.Password.RequiredLength = 8;

            // most of the time you'll want this enabled
            o.SignIn.RequireConfirmedEmail = true;
        })
        .AddUserManager<AccountServices>()
        .AddUserStore<UserStore>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // set the remember me cookies to be for 30 days
        services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.TwoFactorRememberMeScheme, o =>
        {
            o.ExpireTimeSpan = TimeSpan.FromDays(30);
        });

        return services;
    }

    /// <summary>
    /// Adds mapster configs. Note this is the global settings.
    /// We need to keep this in mind so that we don't run into re-compilation issues and potential exception if we repeat configs.
    /// If we need to make modifications then we need to use <see cref="Mapster.TypeAdapterConfig.Fork(Action{TypeAdapterConfig}, string, int)"/>.
    /// See <see href="https://github.com/MapsterMapper/Mapster/wiki/Config-location#entry-point">here</see>
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddMapster(this IServiceCollection services, IEnumerable<Assembly>? assembliesToScan = default)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        services.AddSingleton(config);
        services.AddScoped<IMapper, MapsterMapper.Mapper>();

        config.Scan(assembliesToScan?.ToArray() ?? Array.Empty<Assembly>());

        // set to fast compiler
        config.Compiler = exp => exp.CompileFast();

        // validate before moving on
        config.Compile();

        return services;
    }

    public static IServiceCollection AddMessagingService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection(nameof(EmailOptions)))
            .AddSingleton<IMessagingService, MessagingService>();

        return services;
    }

    public static IServiceCollection AddDataProtection(this IServiceCollection services)
    {
        services.AddDataProtection(o =>
        {
            o.ApplicationDiscriminator = Shared.Constants.System.SharedDataProtectionDiscriminator;
        })
        .PersistKeysToDbContext<AppDbContext>();

        return services;
    }

}
