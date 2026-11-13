using ZelaFix.Application.DTOs;
using ZelaFix.Domain.Entities;

namespace ZelaFix.Application.Repositories;

public interface IChamadoRepository : IRepository<Chamado>
{
    Task<PaginaResponse<ChamadoLeitura>> BuscarAsync(ChamadoFiltro filtro, int usuarioId, CancellationToken ct = default);

    Task<ChamadoLeitura?> ObterLeituraAsync(int id, int usuarioId, CancellationToken ct = default);

    Task<IReadOnlyList<HistoricoLeitura>> ObterHistoricoAsync(int chamadoId, CancellationToken ct = default);

    Task<byte[]?> GetFotoAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyDictionary<int, byte[]>> ObterFotosAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);

    /// <summary>Remove o chamado junto com votos e histórico (o banco não tem ON DELETE CASCADE).</summary>
    Task ExcluirComDependenciasAsync(int id, CancellationToken ct = default);
}