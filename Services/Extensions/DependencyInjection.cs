using FastExpressionCompiler;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Auth;
using Services.DbContexts;
using Services.HelperServices;
using Shared.Mappings;
using Shared.Models.DB.Identity;
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
            .AddScoped(typeof(IGenericDataService<>), typeof(GenericDataService<>))
            .AddMapster(new[] { apiAssembly, typeof(MapsterGenericRegister).Assembly });

        // for a generic template this is good enough. Once its implemented this would need to be adjusted accordingly
        services.AddHttpClient<IMessagingService, MessagingService>();

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

        return doPool ? services.AddDbContextPool<ApplicationDbContext>(optionsConfig) : services.AddDbContext<ApplicationDbContext>(optionsConfig);
    }

    public static IServiceCollection AddEFCoreIdentity(this IServiceCollection services)
    {
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


}
