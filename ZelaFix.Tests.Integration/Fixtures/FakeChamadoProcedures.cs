using Microsoft.EntityFrameworkCore;
using ZelaFix.Application.Exceptions;
using ZelaFix.Application.Repositories;
using ZelaFix.Domain.Entities;
using ZelaFix.Infrastructure.Persistence;

namespace ZelaFix.Tests.Integration.Fixtures;

/// <summary>Substitui os pacotes PL/SQL nos testes (que rodam sem Oracle), mantendo as mesmas regras de negócio.</summary>
public sealed class FakeChamadoProcedures(ZelaFixDbContext db) : IChamadoProcedures
{
    public async Task<int> AbrirChamadoAsync(string titulo, string descricao, byte[] foto,
        int usuarioId, int enderecoId, int categoriaId, int prioridadeId, int orgaoId,
        CancellationToken ct = default)
    {
        var pendente = await db.Status.FirstAsync(s => s.Nome == Status.Pendente, ct);
        var chamado = new Chamado(titulo, descricao, foto, usuarioId, enderecoId, categoriaId, prioridadeId, orgaoId);
        chamado.RegistrarHistorico(pendente.Id, usuarioId, "Ocorrência registrada pelo cidadão.");

        db.Chamados.Add(chamado);
        await db.SaveChangesAsync(ct);
        return chamado.Id;
    }

    public async Task AtualizarStatusAsync(int chamadoId, int statusId, int fiscalId, string observacoes, CancellationToken ct = default)
    {
        var fiscal = await db.Usuarios.FindAsync([fiscalId], ct);
        if (fiscal is null || !fiscal.EhGestor)
            throw new AcessoNegadoException("Operação restrita a gestores.");

        db.Historicos.Add(new HistChamado(chamadoId, statusId, fiscalId, observacoes));
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> RegistrarVotoAsync(int chamadoId, int usuarioId, CancellationToken ct = default)
    {
        var autorId = await db.Chamados.Where(c => c.Id == chamadoId).Select(c => c.UsuarioId).FirstAsync(ct);
        if (autorId == usuarioId)
            throw new AcessoNegadoException("O autor não pode votar no próprio chamado.");

        var atual = await db.Historicos.Where(h => h.ChamadoId == chamadoId)
            .OrderByDescending(h => h.Id).Select(h => h.Status.Nome).FirstAsync(ct);
        if (atual is Status.Concluido or Status.Rejeitado)
            throw new ConflitoException($"Votação encerrada: chamado com status {atual}.");

        var voto = new VotoChamado(chamadoId, usuarioId);
        db.Votos.Add(voto);
        await db.SaveChangesAsync(ct);
        return voto.Id;
    }

    public Task<string> ClassificarCriticidadeBairroAsync(int bairroId, CancellationToken ct = default) =>
        Task.FromResult("BAIXA");

    public Task<string> RelatorioEngajamentoJsonAsync(int bairroId, CancellationToken ct = default) =>
        Task.FromResult("{}");
}