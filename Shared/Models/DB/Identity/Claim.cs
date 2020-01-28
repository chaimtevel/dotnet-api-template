namespace Shared.Models.DB.Identity
{
    public class Claim
    {
        public int ID { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public string Description { get; set; }
    }
}
