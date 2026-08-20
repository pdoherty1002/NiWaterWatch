using System.Linq.Expressions;

namespace NiWaterWatch.Domain.Interfaces;

/// <summary>
/// A generic data-access abstraction over a single entity type. Exists so that services
/// depending on it can be unit tested with a fake implementation, without needing a real
/// database or EF Core involved.
/// </summary>
/// <typeparam name="T">The entity type this repository manages.</typeparam>
/// <typeparam name="TKey">The type of that entity's primary key (e.g. int, Guid).</typeparam>
public interface IRepository<T, TKey> where T : class
{
    /// <summary>Fetches a single entity by its primary key, or null if not found.</summary>
    Task<T?> GetByIdAsync(TKey id);

    /// <summary>Fetches every entity of this type.</summary>
    Task<IReadOnlyList<T>> GetAllAsync();

    /// <summary>Fetches every entity matching the given condition.</summary>
    Task<IReadOnlyList<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate);

    /// <summary>Stages a new entity for insertion. Call <see cref="SaveChangesAsync"/> to actually persist it.</summary>
    Task AddAsync(T entity);

    /// <summary>Stages changes to an existing entity for update.</summary>
    void Update(T entity);

    /// <summary>Stages an entity for deletion.</summary>
    void Remove(T entity);

    /// <summary>Persists all staged changes to the database. Returns true if at least one row was affected.</summary>
    Task<bool> SaveChangesAsync();
}