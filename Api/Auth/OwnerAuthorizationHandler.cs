using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Auth
{
    public class OwnerAuthorizationHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            if(!context.User.Identity.IsAuthenticated)
            {
                return Task.CompletedTask;
            }

            // TODO: add logic to check if this is the owner
            if (context.Resource is AuthorizationFilterContext mvcContext)
            {
                AllowOwnerAttribute isOwner = mvcContext.Filters.OfType<AllowOwnerAttribute>().FirstOrDefault();

                string property = isOwner?.RouteDataProperty ?? "id";
                if (mvcContext.RouteData.Values.TryGetValue(property, out object value))
                {
                    var nameIdentifier = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (value.Equals(nameIdentifier))
                    {
                        AlowPermissions();
                    }
                }
            }

            void AlowPermissions()
            {
                // we may want to filter what permissions we allow
                foreach (var item in context.PendingRequirements.ToArray())
                {
                    context.Succeed(item);
                }
            }

            return Task.CompletedTask;
        }
    }
}
