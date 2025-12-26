using Domain.Abstractions;
using Domain.Entities;
using System.Linq.Expressions;

namespace Domain.Interfaces;

public interface IService<TEntity> where TEntity : EntityBase
{
    /// <summary>
    /// Retrieves a collection of <typeparamref name="TEntity"/> with optional pagination, filtering, and search.
    /// </summary>
    /// <param name="options">
    /// Query options (pagination, filters, sorting, search).
    /// If <c>null</c>, all records will be returned without applying filters or pagination.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="PagedResult{TEntity}"/> containing the retrieved records,
    /// along with pagination metadata and total item count when applicable.
    /// </returns>
    Task<PagedResult<TEntity>> GetAllAsync(
        QueryOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<Result> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task<int> CountEntityAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
}