using Api.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddRouteOptions(this IServiceCollection services)
        {
            return services.AddRouting(routeOptions =>
            {
                routeOptions.ConstraintMap.Add("authprovider", typeof(AuthProviderConstraint));
            });
        }
    }
}
