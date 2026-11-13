namespace ZelaFix.Application.DTOs;

/// <summary>Formato que o app Mobile já espera: <c>{ token, usuario }</c>.</summary>
public sealed record AuthResponse(string Token, DateTime ExpiraEmUtc, UsuarioResponse Usuario);