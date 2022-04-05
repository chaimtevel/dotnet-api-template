using Api.Attributes;
using Microsoft.AspNetCore.Authorization;
using static Shared.Extensions.UserExtensions;

namespace Api.Mvc;

public class EnsureNonMfaHandler : AuthorizationHandler<NonMfaRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NonMfaRequirement requirement)
    {
        // at this point context.HttpContext.User isn't populated yet so we use context.Principal
        static bool HasMfaAuthorize(HttpContext httpContext)
        {
            return (httpContext.GetEndpoint()?.Metadata.OfType<MfaAuthorizeAttribute>().Any())
                .GetValueOrDefault();
        }

        if (context.User.ToAuthenticatedUser().RequiresMfaClaim)
        {
            if(context.Resource is HttpContext httpContext && HasMfaAuthorize(httpContext))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}