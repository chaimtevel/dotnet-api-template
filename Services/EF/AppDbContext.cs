using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DB;
using Shared.Models.DB.Identity;
using static Services.Extensions.EfExtensions;

namespace Services.EF;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<Person> People => Set<Person>();

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureIdentity();

        #region type mappings

        builder.Entity<User>()
            .Property(u => u.PasswordHash)
            .HasColumnType("VARCHAR(100)");

        builder.Entity<DataProtectionKey>()
            .Property(k => k.Xml)
            .HasColumnType("VARCHAR(MAX)");

        #endregion

        // this should be done after the whole model has been built
        builder
            .ConfigureSoftDeletes()
            .ConfigureTracking();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<string>().HaveColumnType("VARCHAR(50)");

        base.ConfigureConventions(configurationBuilder);
    }

}
