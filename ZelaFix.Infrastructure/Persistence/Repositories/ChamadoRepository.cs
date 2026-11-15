using Microsoft.EntityFrameworkCore;
using ZelaFix.Application.DTOs;
using ZelaFix.Application.Repositories;
using ZelaFix.Domain.Entities;

namespace ZelaFix.Infrastructure.Persistence.Repositories;

public sealed class ChamadoRepository(ZelaFixDbContext context)
    : Repository<Chamado>(context), IChamadoRepository
{
    public async Task<PaginaResponse<ChamadoLeitura>> BuscarAsync(
        ChamadoFiltro filtro, int usuarioId, CancellationToken ct = default)
    {
        IQueryable<Chamado> query = Context.Chamados.AsNoTracking();

        if (filtro.CategoriaId is { } categoriaId)
            query = query.Where(c => c.CategoriaId == categoriaId);

        if (filtro.BairroId is { } bairroId)
            query = query.Where(c => c.Endereco.BairroId == bairroId);

        if (filtro.ApenasMeus == true)
            query = query.Where(c => c.UsuarioId == usuarioId);

        if (filtro.Status is { } status)
        {
            // O status atual é o do último registro do histórico (maior Id = mais recente).
            query = query.Where(c => c.Historico
                .OrderByDescending(h => h.Id)
                .Select(h => h.Status.Nome)
                .FirstOrDefault() == status);
        }

        if (filtro.Busca is { } busca)
        {
            var termo = busca.ToLower();
            query = query.Where(c =>
                c.Titulo.ToLower().Contains(termo)
                || c.Descricao.ToLower().Contains(termo)
                || c.Endereco.Rua.ToLower().Contains(termo)
                || c.Endereco.Bairro.Nome.ToLower().Contains(termo));
        }

        var total = await query.LongCountAsync(ct);

        var ordenada = filtro.Ordenar == ChamadoOrdenacao.MaisApoiados
            ? query.OrderByDescending(c => c.Votos.Count()).ThenByDescending(c => c.Id)
            : query.OrderByDescending(c => c.DataAbertura).ThenByDescending(c => c.Id);

        var page = filtro.Page ?? 0;
        var size = filtro.Size ?? 20;
        var itens = await Projetar(ordenada.Skip(page * size).Take(size), usuarioId)
            .ToListAsync(ct);

        return PaginaResponse<ChamadoLeitura>.Criar(itens, total, page, size);
    }

    public Task<ChamadoLeitura?> ObterLeituraAsync(int id, int usuarioId, CancellationToken ct = default) =>
        Projetar(Context.Chamados.AsNoTracking().Where(c => c.Id == id), usuarioId)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<HistoricoLeitura>> ObterHistoricoAsync(int chamadoId, CancellationToken ct = default) =>
        await Context.Historicos.AsNoTracking()
            .Where(h => h.ChamadoId == chamadoId)
            .OrderBy(h => h.Id)
            .Select(h => new HistoricoLeitura(h.Id, h.DataMudanca, h.Status.Nome, h.Observacoes, h.Usuario.Nome))
            .ToListAsync(ct);

    public Task<byte[]?> GetFotoAsync(int id, CancellationToken ct = default) =>
        Context.Chamados.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => c.Foto)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyDictionary<int, byte[]>> ObterFotosAsync(
        IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return new Dictionary<int, byte[]>();

        return await Context.Chamados.AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Foto, ct);
    }

    public async Task ExcluirComDependenciasAsync(int id, CancellationToken ct = default)
    {
        var apoios = await Context.Votos.Where(v => v.ChamadoId == id).ToListAsync(ct);
        var historico = await Context.Historicos.Where(h => h.ChamadoId == id).ToListAsync(ct);
        var chamado = await Context.Chamados.FirstOrDefaultAsync(c => c.Id == id, ct);

        Context.Votos.RemoveRange(apoios);
        Context.Historicos.RemoveRange(historico);
        if (chamado is not null)
            Context.Chamados.Remove(chamado);

        await Context.SaveChangesAsync(ct);
    }

    // Projeção direta para o modelo de leitura: o BLOB da foto nunca entra no SELECT das listagens.
    private static IQueryable<ChamadoLeitura> Projetar(IQueryable<Chamado> query, int usuarioId) =>
        query.Select(c => new ChamadoLeitura(
            c.Id, c.Titulo, c.Descricao,
            c.CategoriaId, c.Categoria.Nome,
            c.PrioridadeId, c.Prioridade.Nome,
            c.OrgaoGovernoId, c.OrgaoGoverno.Nome,
            c.UsuarioId, c.Usuario.Nome,
            c.Historico.OrderByDescending(h => h.Id).Select(h => h.Status.Nome).FirstOrDefault(),
            c.Votos.Count(),
            c.Votos.Count(v => v.UsuarioId == usuarioId),
            c.Endereco.Cep, c.Endereco.Rua, c.Endereco.Numero, c.Endereco.Referencia,
            c.Endereco.Bairro.Nome, c.Endereco.Bairro.Cidade.Nome, c.Endereco.Bairro.Cidade.Estado.Nome,
            c.DataAbertura,
            c.Historico.Max(h => (DateTime?)h.DataMudanca)));
}