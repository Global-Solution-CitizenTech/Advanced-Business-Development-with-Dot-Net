namespace ZelaFix.Domain.Enums;

public static class CargoExtensions
{
    /// <summary>Valor gravado na coluna CARGO e usado na claim "role" (CIDADAO ou GESTOR).</summary>
    public static string ParaPerfil(this Cargo cargo) => cargo.ToString().ToUpperInvariant();
}