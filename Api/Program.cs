using Gelf.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Options;
using static Api.Extensions.PermissionExtensions;
using static Services.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

builder.Services.BuildServices(builder.Configuration);

builder.Logging.BuildLogging(builder.Configuration, builder.Environment);


var app = builder.Build();

app.BuildApp(app.Configuration, app.Environment);

app.Run();



internal static class StartupMethods
{
    public static IServiceCollection BuildServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddMvc()
            .AddNewtonsoftJson(o =>
            {
                o.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                o.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            });

        services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));

        services
            .AddCors()
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                var jwtOptions = new JwtOptions();
                configuration.Bind("JwtOptions", jwtOptions);

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwtOptions.SecretToKey,

                    RequireExpirationTime = true,
                    ValidateLifetime = true
                };
            });

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
                    { scheme, System.Array.Empty<string>() }
                });
        })
        .AddSwaggerGenNewtonsoftSupport();

        // helper service methods
        services.AddApiServices(configuration, typeof(StartupMethods).Assembly)
            .AddApiAuthorization();

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

    public static IApplicationBuilder BuildApp(this IApplicationBuilder app, IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment.IsDevelopment())
        {
            //app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCors(b =>
        {
            string[] allowedOrigins = configuration.GetSection("CorsAllowedOrigins").Get<string[]>() ?? System.Array.Empty<string>();
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

        app.UseExceptionHandler(a =>
        {
            a.Run(async c =>
            {
                c.Response.ContentType = "application/json";
                var ex = c.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

                var serialized = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Message = ex?.Error.Message ?? "Error processing request",
#if DEBUG
                    ex?.Error.StackTrace
#endif
                });

                await c.Response.WriteAsync(serialized);
            });
        });

        app.UseEndpoints(b =>
        {
            b.MapControllers();
        });

        return app;
    }
}
