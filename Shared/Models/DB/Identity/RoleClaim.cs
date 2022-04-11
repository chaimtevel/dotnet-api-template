using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB.Identity;

[Table("RoleClaims")]
public class RoleClaim
{
    public int RoleID { get; set; }
    public virtual Role Role { get; set; } = new();

    public int ClaimID { get; set; }
    public virtual Claim Claim { get; set; } = new();
}