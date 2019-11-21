using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Shared.Models.DB
{
    public class Role : IdentityRole<int>
    {
        public string Description { get; set; }

        public virtual ICollection<IdentityUserRole<int>> UserRoles { get; } = new HashSet<IdentityUserRole<int>>();
        public virtual ICollection<IdentityRoleClaim<int>> RoleClaims { get; } = new HashSet<IdentityRoleClaim<int>>();
    }
}
