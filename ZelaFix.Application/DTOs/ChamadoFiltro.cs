namespace ZelaFix.Application.DTOs;

public enum ChamadoOrdenacao
{
    Recentes = 0,
    MaisApoiados = 1
}

/// <summary>Parâmetros de consulta de <c>GET /chamados</c> (página começa em 0, como no app).</summary>
public sealed class ChamadoFiltro
{
    public const int TamanhoMaximo = 100;

    public int? Page { get; set; } = 0;
    public int? Size { get; set; } = 20;
    public int? CategoriaId { get; set; }
    public int? BairroId { get; set; }
    public string? Status { get; set; }
    public string? Busca { get; set; }
    public bool? ApenasMeus { get; set; }
    public ChamadoOrdenacao? Ordenar { get; set; } = ChamadoOrdenacao.Recentes;

    /// <summary>Inclui a foto (BLOB em Base64) em cada item da lista. Por padrão a lista vem sem foto.</summary>
    public bool? IncluirFoto { get; set; }

    public ChamadoFiltro Normalizado() => new()
    {
        Page = Math.Max(0, Page ?? 0),
        Size = Math.Clamp(Size ?? 20, 1, TamanhoMaximo),
        CategoriaId = CategoriaId,
        BairroId = BairroId,
        Status = string.IsNullOrWhiteSpace(Status) ? null : Status.Trim(),
        Busca = string.IsNullOrWhiteSpace(Busca) ? null : Busca.Trim(),
        ApenasMeus = ApenasMeus,
        Ordenar = Ordenar ?? ChamadoOrdenacao.Recentes,
        IncluirFoto = IncluirFoto == true
    };
}