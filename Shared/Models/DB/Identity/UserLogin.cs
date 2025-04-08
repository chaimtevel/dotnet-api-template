using Shared.Auth;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB.Identity;

[Table("UserLogins")]
public class UserLogin
{
    public int ID { get; set; }

    public int UserID { get; set; }
    public required virtual User User { get; set; }

    [Column(TypeName = "varchar(15)")]
    public AuthProviders LoginProvider { get; set; }
    [Column(TypeName = "varchar(150)")]
    public string ProviderKey { get; set; } = string.Empty;
    public string? ProviderDisplayName { get; set; }
}
