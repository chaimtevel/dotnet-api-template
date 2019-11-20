namespace Shared.Models.DB
{
    public class UserClaim
    {
        public int UserID { get; set; }
        public virtual User User { get; set; }

        public int ClaimID { get; set; }
        public virtual Claim Claim { get; set; }
    }
}
