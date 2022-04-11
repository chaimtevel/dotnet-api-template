using Shared.Models.DB.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB;

public class Person
{
    public int ID { get; set; }
    public string? FirstName { get; set; }
    public string LastName { get; set; } = string.Empty;

    public int? AddressID { get; set; }
    [ForeignKey(nameof(AddressID))]
    public virtual Address? Address { get; set; }

    public virtual ICollection<PersonContactMethod> ContactMethods { get; set; } = new HashSet<PersonContactMethod>();

    [InverseProperty(nameof(Identity.User.Person))]
    public virtual User? User { get; set; }
}