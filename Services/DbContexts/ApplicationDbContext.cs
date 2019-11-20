using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DB;

namespace Services.DbContexts
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().ToTable("Users");
            builder.Entity<Role>().ToTable("Roles");

            // remove the old type mapping first
            builder.Ignore<IdentityRoleClaim<int>>();
            builder.Entity<RoleClaim>()
                .ToTable("RoleClaims")
                .HasKey(rc => new { rc.RoleID, rc.ClaimID });

            // remove the old type mapping first
            builder.Ignore<IdentityUserClaim<int>>();
            builder.Entity<UserClaim>()
                .ToTable("UserClaims")
                .HasKey(uc => new { uc.UserID, uc.ClaimID });

            builder.Ignore<IdentityUserRole<int>>();
            builder.Entity<UserRole>()
                .ToTable("UserRoles")
                .HasKey(ur => new { ur.UserID, ur.RoleID });

            builder.Ignore<IdentityUserLogin<int>>();
            builder.Entity<UserLogin>()
                .ToTable("UserLogins")
                .HasKey(al => new { al.ProviderKey, al.LoginProvider });

            builder.Ignore<IdentityUserToken<int>>();
            builder.Entity<UserToken>()
                .ToTable("UserTokens")
                .HasKey(at => new { at.UserID, at.LoginProvider, at.Name });

        }

    }
}
