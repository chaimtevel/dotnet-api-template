using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Services.EF;
using Shared.Helpers;
using Shared.Options;
using System.Linq.Expressions;
using static Services.Extensions.EfExtensions;
using static Shared.Extensions.UserExtensions;

namespace Services.HelperServices;

internal class GenericDataService<TEntity> : IGenericDataService<TEntity>
        where TEntity : class
{
    public GenericDataService(AppDbContext dbContext
        , IHttpContextAccessor? httpContextAccessor = null)
    {
        DbContext = dbContext;
        HttpContextAccessor = httpContextAccessor;
    }

    public AppDbContext DbContext { get; }
    public IHttpContextAccessor? HttpContextAccessor { get; }

    public IQueryable<TEntity> Entities => DbContext.Set<TEntity>();

    private IQueryable<T> ApplyGlobalFilters<T>(IQueryable<T> query, DbSetFilterOptions options)
        where T : class
    {
        if (options.IgnoreSoftDeleteFilter && query is IQueryable<ISoftDelete>)
        {
            query = query.IgnoreQueryFilters();
        }

        return query;
    }

    public IQueryable<T> AccessData<T>(DbSetFilterOptions? filterOptions = null) where T : class
    {
        filterOptions ??= new DbSetFilterOptions();

        IQueryable<T> query = DbContext.Set<T>();

        return ApplyGlobalFilters<T>(query, filterOptions);
    }


    public Task LoadDataAsync<T>(CancellationToken token = default) where T : class
        => DbContext.Set<T>().LoadAsync();

    public Task<TEntity[]> GetAllAsync(CancellationToken token = default)
        => DbContext.Set<TEntity>().ToArrayAsync(token);

    public Task<TEntity[]> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? includer = default, CancellationToken token = default)
    {
        IQueryable<TEntity> set = DbContext.Set<TEntity>();

        if (includer is not null)
        {
            set = includer.Invoke(set);
        }

        return set.ToArrayAsync();
    }

    public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter, CancellationToken token = default)
        => DbContext.Set<TEntity>().FirstOrDefaultAsync(filter, token);

    public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter
        , CancellationToken token = default
        , Func<IQueryable<TEntity>, IQueryable<TEntity>>? includer = default)
    {
        IQueryable<TEntity> set = DbContext.Set<TEntity>();

        if (includer != default)
        {
            set = includer.Invoke(set);
        }

        return set.FirstOrDefaultAsync(filter, token);
    }

    public Task<TEntity[]> FilterAsync(Expression<Func<TEntity, bool>> filter
        , CancellationToken token = default
        , Func<IQueryable<TEntity>, IQueryable<TEntity>>? includer = default)
    {
        IQueryable<TEntity> set = DbContext.Set<TEntity>();

        if (includer is not null)
        {
            set = includer.Invoke(set);
        }

        return set.Where(filter).ToArrayAsync(token);
    }

    public EntityEntry<TEntity> Add(TEntity entity)
        => DbContext.Set<TEntity>().Add(entity);

    public EntityEntry Add(object entity) => DbContext.Add(entity);

    public void AddRange(IEnumerable<TEntity> entities)
        => DbContext.Set<TEntity>().AddRange(entities);

    public void AddRange(IEnumerable<object> entities) => DbContext.AddRange(entities);

    public EntityEntry<TEntity> Update(TEntity entity) => DbContext.Set<TEntity>().Update(entity);

    public void UpdateRange(IEnumerable<TEntity> entities) => DbContext.Set<TEntity>().UpdateRange(entities);

    public void UpdateRange(IEnumerable<object> entities) => DbContext.UpdateRange(entities);

    public EntityEntry<TEntity> Remove(TEntity entity)
        => DbContext.Set<TEntity>().Remove(entity);

    public EntityEntry Remove(object entity)
        => DbContext.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities)
        => DbContext.Set<TEntity>().RemoveRange(entities);

    public void RemoveRange(IEnumerable<object> entities)
        => DbContext.RemoveRange(entities);

    public Task<int> SaveChangesAsync(CancellationToken token = default)
        => SaveChangesAsync(new(), token);

    public Task<int> SaveChangesAsync(DbSaveOptions options, CancellationToken token = default)
    {
        PrepareModel(options);

        return DbContext.SaveChangesAsync(token);
    }
    
    private void PrepareModel(DbSaveOptions options)
    {
        int? accountId = HttpContextAccessor?.HttpContext?.ToAuthenticatedUser().ID;

        // call to array to avoid catching things changed while we process the entities
        foreach (EntityEntry entry in DbContext.ChangeTracker.Entries().ToArray())
        {
            switch (entry.State)
            {
                case EntityState.Deleted:
                case EntityState.Modified:
                case EntityState.Added:
                    TrackHistory(entry, accountId);
                    break;
                case EntityState.Detached:
                case EntityState.Unchanged:
                default:
                    break;
            }

            // this modifies deletes to modified so do this after we track
            if (options.AutoSetSoftDelete)
            {
                entry.HandleSoftDeletes();
            }

        }
    }

}
