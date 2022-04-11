using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using Shared.Options;

namespace Api.Extensions;

public static class AspNetCoreExtensions
{
    public static IApplicationBuilder AddForwardHeaderMiddleware(this IApplicationBuilder app)
    {
        // see here https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-6.0#forwarded-headers-middleware-options
        var forwardHeaderOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor,
            ForwardLimit = null,
            // this is implemented for cloudflare
            ForwardedForHeaderName = "CF-Connecting-IP"
        };

        app.UseForwardedHeaders(forwardHeaderOptions);

        return app;
    }

    public static BadRequestObjectResult FormatBadRequestResult(this ModelStateDictionary modelState)
    {
        // this code assumes that this is invalid so there's at least one
        // We don't check for null's
        var firstInvalidEntry = modelState.Keys
            .Where(k => modelState[k]?.ValidationState == ModelValidationState.Invalid)
            .Select(k => new
            {
                Code = k,
                Message = modelState[k]?.Errors.FirstOrDefault()?.ErrorMessage
            })
            .FirstOrDefault();

        return new BadRequestObjectResult(new
        {
            status = 400,
            message = firstInvalidEntry?.Message ?? "Unknown Issue",
            code = firstInvalidEntry?.Code ?? "UnknownIssueCode",
            messages = modelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToArray()
        });
    }


    public static JwtBearerOptions ConfigureOptions(this JwtBearerOptions options, IConfiguration configuration)
    {
        var jwtOptions = new JwtOptions();
        configuration.Bind("JwtOptions", jwtOptions);

        options.TokenValidationParameters = new TokenValidationParameters
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

        return options;
    }
}
