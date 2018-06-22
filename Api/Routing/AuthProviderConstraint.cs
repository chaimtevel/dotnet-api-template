using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Auth;
using System;

namespace Api.Routing
{
    public class AuthProviderConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values.TryGetValue(routeKey, out object routeValue))
            {
                if(Enum.TryParse<AuthProviders>(routeValue.ToString(), true, out AuthProviders auth))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
