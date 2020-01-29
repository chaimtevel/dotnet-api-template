using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Shared.Models.DB.Identity
{
    [Table("UserClaims")]
    public class UserClaim 
    {
        public int UserID { get; set; }
        public virtual User User { get; set; }

        public int ClaimID { get; set; }
        public virtual Claim Claim { get; set; }
    }
}
