using ZelaFix.Application.DTOs;

namespace ZelaFix.Application.Services.Interfaces;

public interface ICategoriaService
{
    Task<IReadOnlyList<CategoriaResponse>> GetAllAsync(CancellationToken ct = default);

    Task<CategoriaResponse?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <exception cref="Exceptions.ConflitoException">Já existe categoria com o mesmo nome.</exception>
    Task<CategoriaResponse> CreateAsync(CategoriaRequest request, CancellationToken ct = default);

    /// <returns>A categoria atualizada, ou <c>null</c> se não existir.</returns>
    Task<CategoriaResponse?> UpdateAsync(int id, CategoriaRequest request, CancellationToken ct = default);

    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}