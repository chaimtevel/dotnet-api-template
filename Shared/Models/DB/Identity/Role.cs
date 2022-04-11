using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB.Identity
{
    [Table("Roles")]
    public class Role : IdentityRole<int>
    {
        public string? Description { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; } = new HashSet<UserRole>();
        public virtual ICollection<RoleClaim> RoleClaims { get; } = new HashSet<RoleClaim>();
    }
}
