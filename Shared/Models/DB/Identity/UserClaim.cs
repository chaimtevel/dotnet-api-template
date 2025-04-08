using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB.Identity;

[Table("UserClaims")]
public class UserClaim
{
    public int UserID { get; set; }
    public required virtual User User { get; set; }

    public int ClaimID { get; set; }
    public required virtual Claim Claim { get; set; }
}
