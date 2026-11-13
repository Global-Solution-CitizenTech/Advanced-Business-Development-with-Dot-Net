using System.Linq.Expressions;
using ZelaFix.Domain.Common;

namespace ZelaFix.Application.Repositories;

/// <summary>Contrato genérico de persistência para entidades que herdam de <see cref="BaseEntity"/>.</summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);

    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    Task<T> AddAsync(T entity, CancellationToken ct = default);

    Task<T> UpdateAsync(T entity, CancellationToken ct = default);

    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
}