using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DB.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using static Services.Extensions.EfExtensions;

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

            builder.ConfigureAspNetIdentity();

            #region type mappings

            builder.Entity<User>()
                .Property(u => u.PasswordHash)
                .HasColumnType("VARCHAR(100)");

            #endregion

            // this should be done after the whole model has been built
            builder.SetDefaultStringDbType();
        }

    }
}
