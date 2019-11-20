using Shared.Auth;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB
{
    public class UserLogin
    {
        public int UserID { get; set; }
        [ForeignKey(nameof(UserID))]
        public virtual User User { get; set; }

        [Column(TypeName = "nvarchar(15)")]
        public AuthProviders LoginProvider { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string ProviderKey { get; set; }
        public string ProviderDisplayName { get; set; }

        public override bool Equals(object obj)
        {
            return obj is UserLogin login &&
                   LoginProvider == login.LoginProvider &&
                   (ProviderKey?.Equals(login.ProviderKey, StringComparison.OrdinalIgnoreCase)).GetValueOrDefault();
        }

        public override int GetHashCode() => HashCode.Combine(LoginProvider, ProviderKey);
    }
}
