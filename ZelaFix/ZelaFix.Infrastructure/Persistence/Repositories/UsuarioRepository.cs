using Microsoft.EntityFrameworkCore;
using ZelaFix.Application.Repositories;
using ZelaFix.Domain.Entities;

namespace ZelaFix.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository(ZelaFixDbContext context)
    : Repository<Usuario>(context), IUsuarioRepository
{
    public Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        ComDetalhes().FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<Usuario?> GetComDetalhesAsync(int id, CancellationToken ct = default) =>
        ComDetalhes().FirstOrDefaultAsync(u => u.Id == id, ct);

    private IQueryable<Usuario> ComDetalhes() =>
        Context.Usuarios
            .Include(u => u.Telefone)
            .Include(u => u.Endereco)
                .ThenInclude(e => e.Bairro)
                    .ThenInclude(b => b.Cidade)
                        .ThenInclude(c => c.Estado);
}