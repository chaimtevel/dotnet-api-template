namespace Shared.Models.DB
{
    public class UserRole
    {
        public int UserID { get; set; }
        public virtual User User { get; set; }

        public int RoleID { get; set; }
        public virtual Role Role { get; set; }
    }
}
