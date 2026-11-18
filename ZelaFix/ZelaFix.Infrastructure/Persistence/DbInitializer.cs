using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Enums;

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

            await SemearUsuariosPadraoAsync(db);

            logger.LogInformation("Banco de dados Oracle migrado e tabelas de domínio verificadas/semeadas com sucesso.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao migrar/semear o banco de dados Oracle na inicialização.");
        }
    }

    public static async Task SemearUsuariosPadraoAsync(ZelaFixDbContext db)
    {
        const string emailGestor = "gestor@zelafix.com.br";
        const string emailCidadao = "cidadao@zelafix.com.br";
        const string senhaPadrao = "Password123!";

        var temGestor = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == emailGestor) is not null;
        var temCidadao = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == emailCidadao) is not null;
        if (temGestor && temCidadao)
            return;

        var estado = await db.Estados.FirstOrDefaultAsync(e => e.Nome == "São Paulo");
        if (estado is null)
        {
            estado = new Estado("São Paulo");
            db.Estados.Add(estado);
            await db.SaveChangesAsync();
        }

        var cidade = await db.Cidades.FirstOrDefaultAsync(c => c.Nome == "São Paulo" && c.EstadoId == estado.Id);
        if (cidade is null)
        {
            cidade = new Cidade("São Paulo", estado.Id);
            db.Cidades.Add(cidade);
            await db.SaveChangesAsync();
        }

        var bairro = await db.Bairros.FirstOrDefaultAsync(b => b.Nome == "Sé" && b.CidadeId == cidade.Id);
        if (bairro is null)
        {
            bairro = new Bairro("Sé", cidade.Id);
            db.Bairros.Add(bairro);
            await db.SaveChangesAsync();
        }

        var endereco = await db.Enderecos.FirstOrDefaultAsync(e => e.Cep == "01001000" && e.BairroId == bairro.Id);
        if (endereco is null)
        {
            endereco = new Endereco("01001000", "Praça da Sé", "1", "Marco Zero", bairro.Id);
            db.Enderecos.Add(endereco);
            await db.SaveChangesAsync();
        }

        if (!temGestor)
        {
            var gestor = new Usuario("Gestor Municipal ZelaFix", emailGestor, senhaPadrao, Cargo.Gestor, endereco.Id);
            db.Usuarios.Add(gestor);
            await db.SaveChangesAsync();
            db.Telefones.Add(new Telefone("11", "988880001", gestor.Id));
            await db.SaveChangesAsync();
        }

        if (!temCidadao)
        {
            var cidadao = new Usuario("Cidadão Exemplo ZelaFix", emailCidadao, senhaPadrao, Cargo.Cidadao, endereco.Id);
            db.Usuarios.Add(cidadao);
            await db.SaveChangesAsync();
            db.Telefones.Add(new Telefone("11", "988880002", cidadao.Id));
            await db.SaveChangesAsync();
        }
    }
}
