using Microsoft.EntityFrameworkCore;
using ZelaFix.Application.Repositories;

namespace ZelaFix.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork(ZelaFixDbContext context) : IUnitOfWork
{
    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default)
    {
        // Provedor não relacional (InMemory nos testes) ou transação já aberta: só executa.
        if (!context.Database.IsRelational() || context.Database.CurrentTransaction is not null)
            return await action();

        await using var transacao = await context.Database.BeginTransactionAsync(ct);
        var resultado = await action();
        await transacao.CommitAsync(ct);
        return resultado; // se action lançar, o Dispose da transação faz o rollback
    }
}