using Api.Mvc;
using Gelf.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Shared.Options;
using static Api.Extensions.AspNetCoreExtensions;
using static Api.Extensions.PermissionExtensions;
using static Services.Extensions.DependencyInjection;
using static Shared.Extensions.SerializationExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.BuildServices(builder.Configuration);

builder.Logging.BuildLogging(builder.Configuration, builder.Environment);

var app = builder.Build();

app.BuildApp(app.Configuration, app.Environment)
    .AddForwardHeaderMiddleware();

app.Run();


internal static class StartupMethods
{
    public static IServiceCollection BuildServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddMvc(o => o.Filters.Add(typeof(BadRequestActionFilter)))
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.SetGlobalMvcSettings();
            })
            .ConfigureApiBehaviorOptions(o =>
            {
                o.InvalidModelStateResponseFactory = c => c.ModelState.FormatBadRequestResult();
            });

        services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));

        services
            .AddCors()
            .AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o => o.ConfigureOptions(configuration));

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "My API", Version = "v1" });

            var bearerSchemeName = "Bearer";

            var scheme = new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please insert 'Bearer _token_' into field",
                Name = "Authorization",
                Scheme = bearerSchemeName,
                BearerFormat = "JWT",
                Type = SecuritySchemeType.ApiKey,
                Reference = new OpenApiReference
                {
                    Id = bearerSchemeName,
                    Type = ReferenceType.SecurityScheme
                }
            };

            c.AddSecurityDefinition(bearerSchemeName, scheme);

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { scheme, Array.Empty<string>() }
            });
        });

        // helper service methods
        services.AddApiServices(configuration, typeof(StartupMethods).Assembly)
            .AddApiAuthorization();

        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    public static ILoggingBuilder BuildLogging(this ILoggingBuilder loggingBuilder, IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        loggingBuilder.AddConsole();

        bool debugGraylog = configuration.GetValue<bool>("Logging:GELF:DebugGraylog", false);
        if (!hostEnvironment.IsDevelopment() || debugGraylog)
        {
            loggingBuilder.AddGelf(o =>
            {
                o.LogSource = System.Environment.MachineName;
                o.AdditionalFields.Add("environment", hostEnvironment.EnvironmentName);
            });
        }

        return loggingBuilder;
    }

    public static IApplicationBuilder BuildApp(this WebApplication app, IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment.IsDevelopment())
        {
            //app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        app.UseRouting();

        app.UseCors(b =>
        {
            string[] allowedOrigins = configuration.GetValue("CorsAllowedOrigins", Array.Empty<string>());
            b.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                // this allows the client to send cookies for things such as 2fa
                .AllowCredentials();
        });

        app.UseAuthentication()
            .UseAuthorization()
            .UseSwagger()
            .UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

        app.UseExceptionHandler(a => { });

        app.MapControllers().RequireAuthorization();

        return app;
    }
}
