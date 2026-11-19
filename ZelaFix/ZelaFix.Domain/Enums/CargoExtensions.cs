namespace ZelaFix.Domain.Enums;

public static class CargoExtensions
{
    /// <summary>Valor gravado na coluna CARGO e usado na claim "role" (CIDADAO, GESTOR ou ADMIN).</summary>
    public static string ParaPerfil(this Cargo cargo) => cargo.ToString().ToUpperInvariant();
}