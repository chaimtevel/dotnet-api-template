using Shared.Auth;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB.Identity;

[Table("UserTokens")]
public class UserToken
{
    public int ID { get; set; }

    public int UserID { get; set; }
    [ForeignKey(nameof(UserID))]
    public virtual User User { get; set; } = new();

    [Column(TypeName = "varchar(15)")]
    public AuthProviders LoginProvider { get; set; }
    public string? Name { get; set; }
    public string? Value { get; set; }
}