using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Helpers;
using Shared.Models.DB.Identity;
using System.Linq.Expressions;
using static Shared.Extensions.ReflectionExtensions;

namespace Services.Extensions;

public static class EfExtensions
{
    #region 

    public static ModelBuilder ConfigureIdentity(this ModelBuilder builder)
    {
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

    public static ModelBuilder ConfigureTracking(this ModelBuilder builder)
    {
        foreach (var item in builder.Model.GetEntityTypes().Where(t => t.ClrType.Implements<ITrackEntity>()))
        {
            var entityBuilder = builder.Entity(item.ClrType);

            entityBuilder
                .Property<DateTime>(Shared.Constants.DB.CreatedOnPropertyName)
                .HasDefaultValueSql<DateTime>("GETUTCDATE()");

            entityBuilder
                .Property<int?>(Shared.Constants.DB.CreatedByPropertyName);

            entityBuilder
                .Property<DateTime?>(Shared.Constants.DB.UpdatedOnPropertyName);

            entityBuilder
                .Property<int?>(Shared.Constants.DB.UpdatedByPropertyName);

            // we only need delete props for softdeletes
            if (item.ClrType.Implements<ISoftDelete>())
            {
                entityBuilder
                    .Property<DateTime?>(Shared.Constants.DB.DeletedOnPropertyName);

                entityBuilder
                    .Property<int?>(Shared.Constants.DB.DeletedByPropertyName);
            }
        }

        return builder;
    }

    public static ModelBuilder ConfigureSoftDeletes(this ModelBuilder builder)
    {
        var boolType = typeof(bool);
        var efPropMethod = typeof(Microsoft.EntityFrameworkCore.EF)
            .GetMethod(nameof(Microsoft.EntityFrameworkCore.EF.Property))
            ?.MakeGenericMethod(boolType);
        var softDeleteParamExpression = Expression.Constant(Shared.Constants.DB.SoftDeletePropertyName);
        var softDeleteBoolExpression = Expression.Constant(false);

        if(efPropMethod is null) { return builder; }

        foreach (var item in builder.Model.GetEntityTypes().Where(i => i.ClrType.Implements<ISoftDelete>()))
        {
            // configure shadow property
            var entityBuilder = builder.Entity(item.ClrType);
            entityBuilder.Property<bool>(Shared.Constants.DB.SoftDeletePropertyName);

            // add global filter
            ParameterExpression entityParamExpression = Expression.Parameter(item.ClrType, "entityParam");
            var globalFilter = Expression.Call(efPropMethod, entityParamExpression, softDeleteParamExpression);
            var isSoftDeletedExpression = Expression.Equal(globalFilter, softDeleteBoolExpression);

            entityBuilder.HasQueryFilter(Expression.Lambda(isSoftDeletedExpression, entityParamExpression));
        }

        return builder;
    }

    #endregion

    #region entity entry

    public static EntityEntry HandleSoftDeletes(this EntityEntry entry)
    {
        if (entry.SafelyGetProperty(Shared.Constants.DB.SoftDeletePropertyName) is var softDeleteProp && softDeleteProp is null)
        {
            return entry;
        }

        switch (entry.State)
        {
            case EntityState.Added:
                softDeleteProp.CurrentValue = false;
                break;
            case EntityState.Deleted:
                entry.State = EntityState.Modified;
                softDeleteProp.CurrentValue = true;
                break;
        }

        return entry;
    }

    public static PropertyEntry? SafelyGetProperty(this EntityEntry entry, string propertyName)
        => entry.Properties.FirstOrDefault(e => e.Metadata.Name == propertyName);

    public static EntityEntry TrackHistory(EntityEntry entry, int? accountId)
    {
        // assume this means that this table does not track entities
        if (entry.Entity is not ITrackEntity) { return entry; }

        var entityState = entry.State;
        PropertyEntry? propertyEntry;
        if (entityState == EntityState.Modified)
        {
            propertyEntry = entry.SafelyGetProperty(Shared.Constants.DB.UpdatedOnPropertyName);
            if (propertyEntry != null)
            {
                propertyEntry.CurrentValue = DateTime.UtcNow;
            }

            propertyEntry = entry.SafelyGetProperty(Shared.Constants.DB.UpdatedByPropertyName);
            if (propertyEntry != null && accountId.HasValue)
            {
                propertyEntry.CurrentValue = accountId;
            }
        }
        else if (entityState == EntityState.Added)
        {
            propertyEntry = entry.SafelyGetProperty(Shared.Constants.DB.CreatedOnPropertyName);
            // only set when its the default
            if (propertyEntry?.CurrentValue is DateTime dt && dt == default)
            {
                propertyEntry.CurrentValue = DateTime.UtcNow;
            }

            propertyEntry = entry.SafelyGetProperty(Shared.Constants.DB.CreatedByPropertyName);
            if (propertyEntry != null && accountId.HasValue)
            {
                propertyEntry.CurrentValue = accountId;
            }
        }
        // this only applies to soft deletes
        else if (entityState == EntityState.Deleted)
        {
            propertyEntry = entry.SafelyGetProperty(Shared.Constants.DB.DeletedOnPropertyName);
            if (propertyEntry != null)
            {
                propertyEntry.CurrentValue = DateTime.UtcNow;
            }

            propertyEntry = entry.SafelyGetProperty(Shared.Constants.DB.DeletedByPropertyName);
            if (propertyEntry != null && accountId.HasValue)
            {
                propertyEntry.CurrentValue = accountId;
            }
        }

        return entry;
    }

    #endregion

}