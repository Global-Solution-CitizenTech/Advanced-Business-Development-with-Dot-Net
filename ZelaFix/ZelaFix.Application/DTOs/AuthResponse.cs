namespace ZelaFix.Application.DTOs;

/// <summary>Formato compatível com o app Mobile: expõe tanto 'usuario' quanto 'user'.</summary>
public sealed record AuthResponse(string Token, DateTime ExpiraEmUtc, UsuarioResponse Usuario)
{
    public UsuarioResponse User => Usuario;
}
