using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models.DB
{
    public class User : IdentityUser<int>
    {
        public virtual ICollection<UserClaim> Claims { get; set; } = new HashSet<UserClaim>();
        public virtual ICollection<UserLogin> Logins { get; set; } = new HashSet<UserLogin>();
        public virtual ICollection<UserToken> Tokens { get; set; } = new HashSet<UserToken>();
        public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
    }
}
