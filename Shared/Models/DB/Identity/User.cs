using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Shared.Models.DB.Identity
{
    [Table("Users")]
    public class User : IdentityUser<int>
    {
        public virtual ICollection<UserClaim> Claims { get; set; } = new HashSet<UserClaim>();
        public virtual ICollection<UserLogin> Logins { get; set; } = new HashSet<UserLogin>();
        public virtual ICollection<UserToken> Tokens { get; set; } = new HashSet<UserToken>();
        public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
    }
}
