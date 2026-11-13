using ZelaFix.Domain.Entities;

namespace ZelaFix.Application.DTOs;

public sealed record CategoriaResponse(int Id, string Nome)
{
    public static CategoriaResponse FromDomain(Categoria categoria) => new(categoria.Id, categoria.Nome);
}