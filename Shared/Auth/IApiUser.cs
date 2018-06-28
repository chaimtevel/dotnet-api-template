using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Shared.Auth
{
    public interface IApiUser
    {
        Guid ID { get; }
        string Email { get; }
        string UserName { get; }
        List<Claim> Claims { get; }
    }
}
