namespace ZelaFix.Application.DTOs;

public sealed record PaginaResponse<T>(
    IReadOnlyList<T> Content, long TotalElements, int TotalPages, int Size, int Number,
    bool First, bool Last, bool Empty,
    IReadOnlyList<Link>? Links = null)
{
    public static PaginaResponse<T> Criar(IReadOnlyList<T> itens, long total, int pagina, int tamanho)
    {
        var totalPaginas = tamanho <= 0 ? 0 : (int)Math.Ceiling(total / (double)tamanho);
        return new PaginaResponse<T>(
            itens, total, totalPaginas, tamanho, pagina,
            First: pagina == 0, Last: pagina >= totalPaginas - 1, Empty: itens.Count == 0);
    }

    public PaginaResponse<TOut> Map<TOut>(Func<T, TOut> selector) =>
        new(Content.Select(selector).ToList(), TotalElements, TotalPages, Size, Number, First, Last, Empty, Links);
}