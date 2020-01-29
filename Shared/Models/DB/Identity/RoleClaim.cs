using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Shared.Models.DB.Identity
{
    [Table("RoleClaims")]
    public class RoleClaim
    {
        public int RoleID { get; set; }
        public virtual Role Role { get; set; }

        public int ClaimID { get; set; }
        public virtual Claim Claim { get; set; }
    }
}
