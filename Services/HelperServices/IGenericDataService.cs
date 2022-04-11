using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Options;
using System.Linq.Expressions;

namespace Services.HelperServices;

public interface IDataService
{
    IQueryable<T> AccessData<T>(DbSetFilterOptions? filterOptions = null) where T : class;
    Task LoadDataAsync<T>(CancellationToken token = default) where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    EntityEntry Add(object entity);
    void AddRange(IEnumerable<object> entities);
    void UpdateRange(IEnumerable<object> entities);
    EntityEntry Remove(object entity);
    void RemoveRange(IEnumerable<object> entities);
}

public interface IGenericDataService<TEntity> : IDataService where TEntity : class
{
    IQueryable<TEntity> Entities { get; }
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter, CancellationToken token = default);
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter, CancellationToken token = default, Func<IQueryable<TEntity>, IQueryable<TEntity>>? includer = default);
    Task<TEntity[]> GetAllAsync(CancellationToken token = default);
    Task<TEntity[]> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? includer = default, CancellationToken token = default);
    Task<TEntity[]> FilterAsync(Expression<Func<TEntity, bool>> filter, CancellationToken token = default, Func<IQueryable<TEntity>, IQueryable<TEntity>>? includer = default);
    EntityEntry<TEntity> Add(TEntity entity);
    void AddRange(IEnumerable<TEntity> entities);
    EntityEntry<TEntity> Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);
    EntityEntry<TEntity> Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);
}
