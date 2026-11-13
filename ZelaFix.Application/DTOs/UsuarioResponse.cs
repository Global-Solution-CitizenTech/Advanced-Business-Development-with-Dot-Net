using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Enums;

namespace ZelaFix.Application.DTOs;

public sealed record UsuarioResponse(
    int Id, string Nome, string Email, string Cargo, string? Telefone, string? Bairro, string? Cep)
{
    public static UsuarioResponse FromDomain(Usuario u) => new(
        u.Id, u.Nome, u.Email, u.Cargo.ParaPerfil(),
        u.Telefone?.Completo, u.Endereco?.Bairro?.Nome, u.Endereco?.Cep);
}