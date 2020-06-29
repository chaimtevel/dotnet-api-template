using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DB.Identity;

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

            // remove undused entities
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
        }

    }
}
