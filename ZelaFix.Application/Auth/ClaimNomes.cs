namespace ZelaFix.Application.Auth;

/// <summary>Nomes das claims do token (sem mapeamento automático do .NET).</summary>
public static class ClaimsNomes
{
    public const string Subject = "sub";
    public const string Email = "email";
    public const string Nome = "name";
    public const string Perfil = "role";
}