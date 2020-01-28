using Shared.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Shared.Models.DB.Identity
{
    [Table("UserLogins")]
    public class UserLogin
    {
        public int UserID { get; set; }
        public virtual User User { get; set; }

        [Column(TypeName = "varchar(15)")]
        public AuthProviders LoginProvider { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string ProviderKey { get; set; }
        public string ProviderDisplayName { get; set; }
    }
}
