using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Enums;

namespace ZelaFix.Application.DTOs;

public sealed record UsuarioEnderecoDto(
    int Id, string Cep, string Rua, string Numero, string Bairro, string Cidade, string Estado, string Referencia);

public sealed record UsuarioResponse(
    int Id, string Nome, string Email, string Cargo, string? Telefone, string? Bairro, string? Cep,
    UsuarioEnderecoDto? Endereco = null)
{
    public string Ddd => Telefone != null && Telefone.Length >= 2
        ? new string(Telefone.Where(char.IsDigit).Take(2).ToArray())
        : "11";

    public string NumeroTelefone => Telefone != null
        ? new string(Telefone.Where(char.IsDigit).Skip(2).ToArray())
        : string.Empty;

    public static UsuarioResponse FromDomain(Usuario u) => new(
        u.Id, u.Nome, u.Email, u.Cargo.ParaPerfil(),
        u.Telefone?.Completo, u.Endereco?.Bairro?.Nome, u.Endereco?.Cep,
        u.Endereco is { } e
            ? new UsuarioEnderecoDto(
                e.Id, e.Cep, e.Rua, e.Numero,
                e.Bairro?.Nome ?? string.Empty,
                e.Bairro?.Cidade?.Nome ?? string.Empty,
                e.Bairro?.Cidade?.Estado?.Nome ?? string.Empty,
                e.Referencia == Domain.Entities.Endereco.SemReferencia ? string.Empty : e.Referencia)
            : new UsuarioEnderecoDto(0, string.Empty, string.Empty, "S/N", string.Empty, string.Empty, string.Empty, string.Empty));
}
