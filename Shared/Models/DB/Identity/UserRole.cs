using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Shared.Models.DB.Identity
{
    [Table("UserRoles")]
    public class UserRole
    {
        public int UserID { get; set; }
        public virtual User User { get; set; }

        public int RoleID { get; set; }
        public virtual Role Role { get; set; }
    }
}
