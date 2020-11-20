using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DB.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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
            // set the default type for string to be VARCHAR(50)
            var stringType = typeof(string);
            var columnAtribute = typeof(ColumnAttribute);
            foreach (var property in builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == stringType))
            {
                if (property.GetMaxLength() == null && !property.PropertyInfo.GetCustomAttributes(columnAtribute, true).Any())
                {
                    property.SetColumnType("VARCHAR(50)");
                    property.SetMaxLength(50);
                }
            }

            base.OnModelCreating(builder);

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

            #region type mappings

            builder.Entity<User>()
                .Property(u => u.PasswordHash)
                .HasColumnType("VARCHAR(100)");

            #endregion

        }

    }
}
