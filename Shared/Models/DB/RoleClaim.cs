using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB
{
    public class RoleClaim
    {
        public int RoleID { get; set; }
        public virtual Role Role { get; set; }

        public int ClaimID { get; set; }
        public virtual Claim Claim { get; set; }
    }
}
