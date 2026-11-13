using ZelaFix.Application.DTOs;

namespace ZelaFix.Application.Services.Interfaces;

public interface IAuthService
{
    /// <exception cref="Exceptions.ConflitoException">E-mail já cadastrado.</exception>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    /// <returns>O token e o perfil, ou <c>null</c> se e-mail ou senha estiverem incorretos.</returns>
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);

    Task<UsuarioResponse?> GetPerfilAsync(int usuarioId, CancellationToken ct = default);
}