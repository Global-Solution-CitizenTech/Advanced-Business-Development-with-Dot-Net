using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ZelaFix.Application.Repositories;
using ZelaFix.Domain.Common;

namespace ZelaFix.Infrastructure.Persistence.Repositories;

/// <summary>Implementação genérica EF Core de <see cref="IRepository{T}"/>.</summary>
public class Repository<T>(ZelaFixDbContext context) : IRepository<T> where T : BaseEntity
{
    protected ZelaFixDbContext Context { get; } = context;

    private readonly DbSet<T> _set = context.Set<T>();

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await _set.AsNoTracking().OrderBy(e => e.Id).ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await _set.AsNoTracking().Where(predicate).ToListAsync(ct);

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _set.FindAsync([id], ct);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await _set.FirstOrDefaultAsync(predicate, ct);

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct);
        await Context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken ct = default)
    {
        _set.Update(entity);
        await Context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _set.FindAsync([id], ct);
        if (entity is null)
            return false;

        _set.Remove(entity);
        await Context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await _set.AnyAsync(predicate, ct);
}