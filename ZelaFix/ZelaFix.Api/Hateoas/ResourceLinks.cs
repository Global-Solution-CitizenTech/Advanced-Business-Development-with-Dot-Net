using ZelaFix.Application.DTOs;

namespace ZelaFix.Api.Hateoas;

/// <summary>
/// Constrói links de hipermídia (HATEOAS) para recursos e coleções da API ZelaFix.
/// </summary>
public static class ResourceLinks
{
    private const string RotaCategorias = "/api/v1/categorias";
    private const string RotaChamados = "/api/v1/chamados";

    public static Link LinkTo(string href, string rel, string method) => new(href, rel, method);

    public static CategoriaResponse Enrich(CategoriaResponse categoria)
    {
        var links = new List<Link>
        {
            LinkTo($"{RotaCategorias}/{categoria.Id}", "self", "GET"),
            LinkTo($"{RotaCategorias}/{categoria.Id}", "atualizar", "PUT"),
            LinkTo($"{RotaCategorias}/{categoria.Id}", "excluir", "DELETE"),
            LinkTo($"{RotaChamados}?categoriaId={categoria.Id}", "chamados", "GET"),
            LinkTo(RotaCategorias, "collection", "GET")
        };
        return categoria with { Links = links };
    }

    public static IReadOnlyList<CategoriaResponse> EnrichAll(IReadOnlyList<CategoriaResponse> categorias) =>
        categorias.Select(Enrich).ToList();

    public static ChamadoResponse Enrich(ChamadoResponse chamado)
    {
        var links = new List<Link>
        {
            LinkTo($"{RotaChamados}/{chamado.Id}", "self", "GET"),
            LinkTo($"{RotaChamados}/{chamado.Id}", "atualizar", "PUT"),
            LinkTo($"{RotaChamados}/{chamado.Id}", "excluir", "DELETE"),
            LinkTo($"{RotaChamados}/{chamado.Id}/status", "alterar-status", "PATCH"),
            LinkTo($"{RotaChamados}/{chamado.Id}/apoiar", "apoiar", "POST"),
            LinkTo($"{RotaChamados}/{chamado.Id}/apoiar", "remover-apoio", "DELETE"),
            LinkTo($"{RotaCategorias}/{chamado.CategoriaId}", "categoria", "GET"),
            LinkTo(RotaChamados, "collection", "GET")
        };
        return chamado with { Links = links };
    }

    public static PaginaResponse<ChamadoResponse> Enrich(PaginaResponse<ChamadoResponse> pagina)
    {
        var itensEnriquecidos = pagina.Content.Select(Enrich).ToList();
        var links = new List<Link>
        {
            LinkTo($"{RotaChamados}?page={pagina.Number}&size={pagina.Size}", "self", "GET"),
            LinkTo($"{RotaChamados}?page=0&size={pagina.Size}", "first", "GET")
        };

        if (pagina.Number > 0)
            links.Add(LinkTo($"{RotaChamados}?page={pagina.Number - 1}&size={pagina.Size}", "prev", "GET"));

        if (pagina.Number < pagina.TotalPages - 1)
            links.Add(LinkTo($"{RotaChamados}?page={pagina.Number + 1}&size={pagina.Size}", "next", "GET"));

        if (pagina.TotalPages > 0)
            links.Add(LinkTo($"{RotaChamados}?page={Math.Max(0, pagina.TotalPages - 1)}&size={pagina.Size}", "last", "GET"));

        return new PaginaResponse<ChamadoResponse>(
            itensEnriquecidos,
            pagina.TotalElements,
            pagina.TotalPages,
            pagina.Size,
            pagina.Number,
            pagina.First,
            pagina.Last,
            pagina.Empty,
            links);
    }
}
