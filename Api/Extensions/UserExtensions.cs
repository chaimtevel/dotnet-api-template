using Api.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;
using System.Security.Claims;

namespace Api.Extensions
{
    public static class UserExtensions
    {

        public static IApiUser ToApiUser(this ClaimsPrincipal principal)
        {
            return new ApiUser
            {
                ID = principal.FindFirstValue(ClaimTypes.NameIdentifier).ToInt(),
                Email = principal.FindFirstValue(ClaimTypes.Email),
                UserName = principal.FindFirstValue(ClaimTypes.Name)
            };
        }

        public static IApiUser ToApiUser(this ControllerBase controller) => ToApiUser(controller.User);
        public static IApiUser ToApiUser(this HttpContext context) => ToApiUser(context.User);

    }
}
