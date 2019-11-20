using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB
{
    public class UserToken
    {
        public int UserID { get; set; }
        [ForeignKey(nameof(UserID))]
        public virtual User User { get; set; }

        public string LoginProvider { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
