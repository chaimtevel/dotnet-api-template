using Shared.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Shared.Models.DB.Identity
{
    [Table("UserTokens")]
    public class UserToken
    {
        public int ID { get; set; }

        public int UserID { get; set; }
        public virtual User User { get; set; }

        [Column(TypeName = "varchar(15)")]
        public AuthProviders LoginProvider { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
