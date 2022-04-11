namespace Shared.Models.API;

public class UserAPI : PersonAPI
{
    public int? UserID { get; set; }
    public string Password { get; init; } = string.Empty;
    public IEnumerable<int> RoleIDs { get; init; } = new HashSet<int>();
}
