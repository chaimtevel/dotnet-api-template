using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB;

[Table("Addresses")]
public class Address
{
    public int ID { get; set; }
    [Column(TypeName = "VARCHAR(100)")]
    public string Address1 { get; set; } = string.Empty;
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string Zip { get; set; } = string.Empty;

    public int? StateID { get; set; }
    [ForeignKey(nameof(StateID))]
    public virtual State? State { get; set; }
}