using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB.Identity;

[Table("UserRoles")]
public class UserRole
{
    public int UserID { get; set; }
    public required virtual User User { get; set; }

    public int RoleID { get; set; }
    public required virtual Role Role { get; set; }
}
