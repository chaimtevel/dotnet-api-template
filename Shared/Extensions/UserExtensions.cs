using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;
using Shared.Models.DB.Identity;
using System.Security.Claims;

namespace Shared.Extensions
{
    public static class UserExtensions
    {

        public static AuthenticatedUser ToAuthenticatedUser(this ClaimsPrincipal principal) => new(principal);
        public static AuthenticatedUser ToAuthenticatedUser(this ControllerBase controller) => ToAuthenticatedUser(controller.User);
        public static AuthenticatedUser ToAuthenticatedUser(this HttpContext context) => ToAuthenticatedUser(context.User);

        public static List<System.Security.Claims.Claim> GetBaseClaims(this User user)
        {
            return new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(ClaimTypes.Email, user.Email),
                new System.Security.Claims.Claim(ClaimTypes.Name, user.Email),
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // just an example
            };
        }
    }
}
