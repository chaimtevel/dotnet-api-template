using FastExpressionCompiler;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services.AppServices;
using Services.Auth;
using Services.EF;
using Services.HelperServices;
using Shared.Mappings;
using Shared.Models.DB.Identity;
using Shared.Options;
using StackExchange.Redis;
using System.Reflection;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using static Shared.Extensions.TaskExtensions;
using Role = Shared.Models.DB.Identity.Role;

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
            .AddMapster([apiAssembly, typeof(MapsterGenericRegister).Assembly])
            .AddDataProtection()
            .AddScoped(typeof(IGenericDataService<>), typeof(GenericDataService<>))
            .AddScoped<IPersonService, PersonService>()
            .AddScoped<IGeoService, GeoService>()
            .AddRedis()
            .AddCaching();

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

    public static IServiceCollection AddDb(this IServiceCollection services, IConfiguration configuration)
    {
        // modify for the appropriate db engine
        void optionsConfig(DbContextOptionsBuilder o)
        {
            // important - we add this in this scope as DI only loads all configs lazily
            // see the add Redis method
            string connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            o.UseSqlServer(connectionString, o => o.EnableRetryOnFailure().UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .UseLazyLoadingProxies();
        }

        return services.AddDbContext<AppDbContext>(optionsConfig);
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
    /// Adds Mapster configs. Note this is the global settings.
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

    public static IServiceCollection AddRedis(this IServiceCollection services)
    {
        // Register Redis connection multiplexer as a singleton
        return services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("Redis") ?? "localhost";
            return ConnectionMultiplexer.Connect(connectionString);
        });
    }

    public static IServiceCollection AddCaching(this IServiceCollection services)
    {
        services.AddFusionCache()
            .WithSystemTextJsonSerializer(Shared.Constants.Serialization.DefaultSystemJsonOptions)
            .WithDistributedCache(sp =>
            {
                // Doing it manually as using the built-in services.AddStackExchangeRedisCache does not allow for connection re-use
                return new RedisCache(new RedisCacheOptions
                {
                    ConnectionMultiplexerFactory = () => sp.GetRequiredService<IConnectionMultiplexer>().WrapInTask()
                });
            })
            .WithBackplane(sp =>
            {
                // Doing it manually as using the built-in FusionCache.WithStackExchangeRedisBackplane does not allow for connection re-use
                var options = new RedisBackplaneOptions
                {
                    ConnectionMultiplexerFactory = () => sp.GetRequiredService<IConnectionMultiplexer>().WrapInTask()
                };

                ILogger<RedisBackplane> logger = sp.GetRequiredService<ILogger<RedisBackplane>>();
                return new RedisBackplane(options, logger);
            })
            .WithOptions(o =>
            {
                // We may want to add versioning to this - see https://github.com/ZiggyCreatures/FusionCache/blob/main/docs/Backplane.md#-wire-format-versioning
                o.BackplaneChannelPrefix = "cache-backplane:";
                o.CacheKeyPrefix = "cache:";
            })
            .WithDefaultEntryOptions(o =>
            {
                var defaultDuration = TimeSpan.FromHours(1);
                o.Duration = defaultDuration;
                o.DistributedCacheDuration = defaultDuration;

                // Fail safe duration cannot be less than than the default duration
                o.FailSafeMaxDuration = TimeSpan.FromHours(3);
                o.DistributedCacheFailSafeMaxDuration = TimeSpan.FromHours(3);
                o.FailSafeThrottleDuration = TimeSpan.FromHours(1);

                o.IsFailSafeEnabled = true;
            });

        return services
            .AddSingleton<ICacheService, InternalCacheService>();
    }
}
