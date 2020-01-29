using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB.Identity
{
    [Table("Claims")]
    public class Claim
    {
        public int ID { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public string Description { get; set; }
    }
}
