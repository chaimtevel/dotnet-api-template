using Api.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Extensions;

public static class PermissionExtensions
{
    public static IServiceCollection AddApiAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, EnsureNonMfaHandler>();

        services.AddAuthorizationCore(o =>
        {
            var newPolicyBuilder = new AuthorizationPolicyBuilder(o.DefaultPolicy);
            newPolicyBuilder.AddRequirements(new NonMfaRequirement());

            // add any global requirements
            newPolicyBuilder.AddAuthenticationSchemes(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme);
            o.DefaultPolicy = newPolicyBuilder.Build();
        });

        return services;
    }
}
