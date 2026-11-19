using System.Security.Claims;
using ZelaFix.Application.Auth;
using ZelaFix.Domain.Enums;

namespace ZelaFix.Api.Security;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Id do usuário autenticado, lido da claim "sub" do token.</summary>
    public static int? ObterUsuarioId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimsNomes.Subject), out var id) ? id : null;

    public static int ExigirUsuarioId(this ClaimsPrincipal user) =>
        user.ObterUsuarioId() ?? throw new UnauthorizedAccessException("O token não identifica o usuário.");

    public static bool EhGestor(this ClaimsPrincipal user) =>
        user.IsInRole(Cargo.Gestor.ParaPerfil()) || user.IsInRole(Cargo.Admin.ParaPerfil());
}