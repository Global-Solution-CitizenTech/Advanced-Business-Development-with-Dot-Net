namespace ZelaFix.Application.Repositories;

/// <summary>Executa várias escritas como uma transação atômica.</summary>
public interface IUnitOfWork
{
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default);
}