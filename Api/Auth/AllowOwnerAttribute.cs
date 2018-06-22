using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Api.Auth
{
    public class AllowOwnerAttribute : Attribute, IFilterMetadata
    {
        /// <summary>
        /// <see cref="OwnerAuthorizationHandler"/> will default to check the id property in the route
        /// Set this property to specify a different property
        /// </summary>
        public string RouteDataProperty { get; set; }
    }
}
