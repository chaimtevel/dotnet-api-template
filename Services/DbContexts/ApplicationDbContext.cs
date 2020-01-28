using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DB;
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

            builder.Entity<Claim>();

            builder.Entity<User>();

            builder.Entity<Role>();

            builder.Entity<UserClaim>();

            builder.Entity<UserLogin>();

            builder.Entity<UserToken>();

            builder.Entity<RoleClaim>();

            builder.Entity<UserRole>();
        }

    }
}
