using ZelaFix.Domain.Entities;

namespace ZelaFix.Application.Repositories;

public interface IUsuarioRepository : IRepository<Usuario>
{
    /// <summary>Busca pelo e-mail (já normalizado em minúsculas), carregando telefone e endereço.</summary>
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<Usuario?> GetComDetalhesAsync(int id, CancellationToken ct = default);
}