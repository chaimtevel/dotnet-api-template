using Api.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Services.Extensions;
using Shared.Options;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonOptions(o =>
                {
                    o.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    o.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.Configure<JwtOptions>(Configuration.GetSection(nameof(JwtOptions)));

            services
                .AddApiServices(Configuration)
                .AddCors()
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(o =>
                {
                    // lets not build the main service provider yet (unsure of what the side-affects of doing that are)
                    ServiceCollection collection = new ServiceCollection();
                    collection.AddOptions();
                    collection.Configure<JwtOptions>(Configuration.GetSection(nameof(JwtOptions)));
                    var sp = collection.BuildServiceProvider();
                    var options = sp.GetService<IOptions<JwtOptions>>().Value;

                    o.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = options.Issuer,

                        ValidateAudience = true,
                        ValidAudience = options.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = options.SecretToKey,

                        RequireExpirationTime = true,
                        ValidateLifetime = true

                    };

                });

            // allows users not allowed by roles/policies to access their own resources
            services.AddSingleton<IAuthorizationHandler, OwnerAuthorizationHandler>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "My API", Version = "v1" });

                c.AddSecurityDefinition(
                    "Bearer"
                    , new ApiKeyScheme
                    {
                        In = "header",
                        Description = "Please insert 'Bearer _token_' into field",
                        Name = "Authorization",
                        Type = "apiKey"
                    });

                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", new string[] { } }
                });

            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseExceptionHandler(a =>
            {
                a.Run(async c =>
                {
                    c.Response.ContentType = "application/json";
                    var ex = c.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

                    var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        Message = ex?.Error.Message ?? "Error processing request",
#if DEBUG
                        ex?.Error.StackTrace
#endif
                    });

                    await c.Response.WriteAsync(serialized);
                });
            });

            app.UseCors(b =>
            {
                string[] allowedOrigins = Configuration.GetSection("CorsAllowedOrigins").Get<string[]>();
                b.WithOrigins(allowedOrigins ?? new string[0])
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });

            app.UseAuthentication()
                .UseMvc()
                .UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                });

        }
    }
}
