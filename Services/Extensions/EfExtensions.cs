using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DB.Identity;
using System.Linq;

namespace Services.Extensions
{
    public static class EfExtensions
    {
        public static ModelBuilder SetDefaultStringDbType(this ModelBuilder builder)
        {
            var stringType = typeof(string);

            foreach (var property in builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == stringType))
            {
                if (string.IsNullOrWhiteSpace(property.GetColumnType()) && property.GetMaxLength() == null)
                {
                    property.SetColumnType("VARCHAR(50)");
                }
            }

            return builder;
        }

        public static ModelBuilder ConfigureAspNetIdentity(this ModelBuilder builder)
        {
            // remove unused entities
            builder.Ignore<IdentityRoleClaim<int>>();
            builder.Ignore<IdentityUserClaim<int>>();
            builder.Ignore<IdentityUserRole<int>>();
            builder.Ignore<IdentityUserLogin<int>>();
            builder.Ignore<IdentityUserToken<int>>();

            builder.Entity<Claim>();

            builder.Entity<User>().ToTable("Users");

            builder.Entity<Role>().ToTable("Roles");

            builder.Entity<UserClaim>()
                .HasKey(uc => new { uc.UserID, uc.ClaimID });

            builder.Entity<UserLogin>();

            builder.Entity<UserToken>();

            builder.Entity<RoleClaim>()
                .HasKey(rc => new { rc.RoleID, rc.ClaimID });

            builder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserID, ur.RoleID });

            return builder;
        }
    }
}
