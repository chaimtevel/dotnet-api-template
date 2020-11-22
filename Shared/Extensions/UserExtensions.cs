using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;
using System;
using System.Security.Claims;

namespace Shared.Extensions
{
    public static class UserExtensions
    {

        public static AuthenticatedUser ToAuthenticatedUser(this ClaimsPrincipal principal)
            => new AuthenticatedUser(principal);

        public static AuthenticatedUser ToApiUser(this ControllerBase controller) => ToAuthenticatedUser(controller.User);
        public static AuthenticatedUser ToApiUser(this HttpContext context) => ToAuthenticatedUser(context.User);

    }
}
