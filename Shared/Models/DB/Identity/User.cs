using Microsoft.AspNetCore.Identity;
using Shared.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB.Identity;

public class User : IdentityUser<int>, ITrackEntity
{
    /// <summary>
    /// This is the email used for logins
    /// This should be combined with emails from <see cref="Shared.Models.DB.PersonContactMethod"/> to get a complete list of emails for a user
    /// </summary>
    public override string Email { get; set; } = string.Empty;
    public override string NormalizedEmail { get; set; } = string.Empty;

    public int? PersonID { get; set; }
    [ForeignKey(nameof(PersonID))]
    public virtual Person? Person { get; set; }

    public virtual ICollection<UserClaim> Claims { get; set; } = new HashSet<UserClaim>();
    public virtual ICollection<UserLogin> Logins { get; set; } = new HashSet<UserLogin>();
    public virtual ICollection<UserToken> Tokens { get; set; } = new HashSet<UserToken>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
}