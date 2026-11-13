using ZelaFix.Application.DTOs;

namespace ZelaFix.Application.Services.Interfaces;

public interface IChamadoService
{
    Task<PaginaResponse<ChamadoResponse>> ListarAsync(ChamadoFiltro filtro, int usuarioId, CancellationToken ct = default);

    Task<ChamadoResponse?> ObterAsync(int id, int usuarioId, CancellationToken ct = default);

    Task<ChamadoResponse> CriarAsync(CriarChamadoRequest request, int usuarioId, CancellationToken ct = default);

    /// <exception cref="Exceptions.AcessoNegadoException">Nem autor nem gestor.</exception>
    Task<ChamadoResponse?> AtualizarAsync(int id, AtualizarChamadoRequest request, int usuarioId, bool ehGestor, CancellationToken ct = default);

    Task<ChamadoResponse?> AlterarStatusAsync(int id, AlterarStatusRequest request, int gestorId, CancellationToken ct = default);

    Task<ChamadoResponse?> ApoiarAsync(int id, int usuarioId, CancellationToken ct = default);

    Task<ChamadoResponse?> RemoverApoioAsync(int id, int usuarioId, CancellationToken ct = default);

    Task<bool> ExcluirAsync(int id, int usuarioId, bool ehGestor, CancellationToken ct = default);
}