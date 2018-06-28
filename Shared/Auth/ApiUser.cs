using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Shared.Auth
{
    public class ApiUser : IApiUser
    {
        public Guid ID { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }

        public List<Claim> Claims { get; } = new List<Claim>();
    }
}
