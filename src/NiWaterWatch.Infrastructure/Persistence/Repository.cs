using Microsoft.EntityFrameworkCore;
using NiWaterWatch.Domain.Interfaces;

namespace NiWaterWatch.Infrastructure.Persistence;

/// <summary>
/// EF Core-backed implementation of <see cref="IRepository{T, TKey}"/>, wrapping
/// a <see cref="AppDbContext"/>'s <see cref="DbSet{T}"/> for the given entity type.
/// </summary>
public class Repository<T, TKey> : IRepository<T, TKey> where T : class
{
    // The shared database context this repository operates through.
    private readonly AppDbContext _context;

    // The specific table (DbSet) for entity type T within that context.
    private readonly DbSet<T> _dbSet;

    /// <summary>Creates a repository bound to the given database context.</summary>
    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(TKey id) => await _dbSet.FindAsync(id);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> GetAllAsync() => await _dbSet.ToListAsync();

    /// <inheritdoc/>
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    /// <inheritdoc/>
    public void Update(T entity) => _dbSet.Update(entity);

    /// <inheritdoc/>
    public void Remove(T entity) => _dbSet.Remove(entity);

    /// <inheritdoc/>
    public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;
}