using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB
{
    [Table("Claims")]
    public class Claim
    {
        public int ID { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public string Description { get; set; }

        public virtual ICollection<UserClaim> Users { get; } = new HashSet<UserClaim>();
        public virtual ICollection<RoleClaim> Roles { get; } = new HashSet<RoleClaim>();
    }
}
