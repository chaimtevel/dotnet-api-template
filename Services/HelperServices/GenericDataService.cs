using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Services.DbContexts;
using System.Linq.Expressions;

namespace Services.HelperServices;

internal class GenericDataService<TEntity> : IGenericDataService<TEntity>
        where TEntity : class
{
    public GenericDataService(ApplicationDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public ApplicationDbContext DbContext { get; }

    public IQueryable<TEntity> Entities => DbContext.Set<TEntity>();

    public IQueryable<T> AccessData<T>() where T : class => DbContext.Set<T>();

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
        => DbContext.SaveChangesAsync(token);

}
