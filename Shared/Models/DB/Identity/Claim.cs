using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB.Identity;

[Table("Claims")]
public class Claim
{
    public int ID { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
    public string? Description { get; set; }
}