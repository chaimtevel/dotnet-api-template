using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Services.Extensions;
using Shared.Options;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebHostEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(o => o.EnableEndpointRouting = false)
                .AddNewtonsoftJson(o =>
                {
                    o.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    o.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

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
                    var jwtOptions = new JwtOptions();
                    Configuration.Bind("JwtOptions", jwtOptions);

                    o.TokenValidationParameters = new TokenValidationParameters()
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

                c.AddSecurityDefinition(
                    "Bearer"
                    , new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Please insert 'Bearer _token_' into field",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey
                    });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
        {
            if (WebHostEnvironment.IsDevelopment())
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
