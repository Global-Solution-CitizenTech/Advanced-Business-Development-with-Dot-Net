namespace ZelaFix.Application.Auth;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    public const int TamanhoMinimoChave = 32;

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "ZelaFix.API";
    public string Audience { get; set; } = "ZelaFix.Clients";
    public int ExpirationMinutes { get; set; } = 120;

    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Key) || Key.Length < TamanhoMinimoChave)
            throw new InvalidOperationException(
                $"Jwt:Key ausente ou com menos de {TamanhoMinimoChave} caracteres. " +
                "Defina com 'dotnet user-secrets set \"Jwt:Key\" \"...\"' ou pela variável de ambiente Jwt__Key.");

        if (ExpirationMinutes <= 0)
            throw new InvalidOperationException("Jwt:ExpirationMinutes deve ser maior que zero.");
    }
}