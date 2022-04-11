using Shared.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB;

[Table("PeopleContactMethods")]
public class PersonContactMethod
{
    public int ID { get; set; }
    [Column(TypeName = "VARCHAR(320)")]
    public string Value { get; set; } = string.Empty;
    public ContactMethodType Type { get; set; }

    public int PersonID { get; set; }
    [ForeignKey(nameof(PersonID))]
    public virtual Person? Person { get; set; }
}