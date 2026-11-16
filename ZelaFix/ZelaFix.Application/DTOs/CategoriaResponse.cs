using ZelaFix.Domain.Entities;

namespace ZelaFix.Application.DTOs;

public sealed record CategoriaResponse(int Id, string Nome, IReadOnlyList<Link>? Links = null)
{
    public static CategoriaResponse FromDomain(Categoria categoria) => new(categoria.Id, categoria.Nome);
}