using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Enums;

namespace ZelaFix.Application.DTOs;

public sealed record UsuarioEnderecoDto(
    int Id, string Cep, string Rua, string Numero, string Bairro, string Cidade, string Estado, string Referencia);

public sealed record UsuarioResponse(
    int Id, string Nome, string Email, string Cargo, string? Telefone, string? Bairro, string? Cep)
{
    public string Ddd => Telefone != null && Telefone.Length >= 2
        ? new string(Telefone.Where(char.IsDigit).Take(2).ToArray())
        : "11";

    public string NumeroTelefone => Telefone != null
        ? new string(Telefone.Where(char.IsDigit).Skip(2).ToArray())
        : string.Empty;

    public UsuarioEnderecoDto Endereco => new(
        0, Cep ?? string.Empty, string.Empty, "S/N", Bairro ?? string.Empty, string.Empty, string.Empty, string.Empty);

    public static UsuarioResponse FromDomain(Usuario u) => new(
        u.Id, u.Nome, u.Email, u.Cargo.ParaPerfil(),
        u.Telefone?.Completo, u.Endereco?.Bairro?.Nome, u.Endereco?.Cep);
}
