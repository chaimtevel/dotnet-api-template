using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models.DB
{
    public class User : IdentityUser<int>
    {
        public virtual ICollection<IdentityUserClaim<int>> Claims { get; set; } = new HashSet<IdentityUserClaim<int>>();
        public virtual ICollection<IdentityUserLogin<int>> Logins { get; set; } = new HashSet<IdentityUserLogin<int>>();
        public virtual ICollection<IdentityUserToken<int>> Tokens { get; set; } = new HashSet<IdentityUserToken<int>>();
        public virtual ICollection<IdentityUserRole<int>> UserRoles { get; set; } = new HashSet<IdentityUserRole<int>>();
    }
}
