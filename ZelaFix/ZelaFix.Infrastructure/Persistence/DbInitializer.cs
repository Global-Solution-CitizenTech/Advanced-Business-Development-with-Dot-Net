using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZelaFix.Domain.Entities;

namespace ZelaFix.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ZelaFixDbContext>();

        if (!db.Database.IsRelational())
            return;

        try
        {
            await db.Database.MigrateAsync();

            if (!await db.Status.AnyAsync())
                db.Status.AddRange(Status.Todos.Select(nome => new Status(nome)));

            if (!await db.Prioridades.AnyAsync())
                db.Prioridades.AddRange(Prioridade.Nomes.Select(nome => new Prioridade(nome)));

            if (!await db.OrgaosGoverno.AnyAsync())
                db.OrgaosGoverno.AddRange(OrgaoGoverno.Nomes.Select(nome => new OrgaoGoverno(nome)));

            if (!await db.Categorias.AnyAsync())
                db.Categorias.AddRange(Categoria.Nomes.Select(nome => new Categoria(nome)));

            if (db.ChangeTracker.HasChanges())
                await db.SaveChangesAsync();

            logger.LogInformation("Banco de dados Oracle migrado e tabelas de domínio verificadas/semeadas com sucesso.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao migrar/semear o banco de dados Oracle na inicialização.");
        }
    }
}
