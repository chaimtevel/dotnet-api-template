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
                }
            }

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
