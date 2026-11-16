using ZelaFix.Domain.Common;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Domain.Entities;

public sealed class Categoria : BaseEntity
{
    public static readonly IReadOnlyList<string> Nomes =
    [
        "ILUMINACAO_PUBLICA",
        "VIAS_E_PAVIMENTACAO",
        "DRENAGEM_E_ESGOTO",
        "LIMPEZA_URBANA",
        "ARVORES_E_PARQUES",
        "SINALIZACAO_E_TRANSITO"
    ];

    public string Nome { get; private set; } = string.Empty;

    private Categoria() { }

    public Categoria(string nome) => Atualizar(nome);

    public void Atualizar(string nome)
    {
        var codigo = Validacao.Texto(nome, "O nome da categoria", 50).ToUpperInvariant();
        if (!Nomes.Contains(codigo))
            throw new DomainException($"Categoria inválida. Valores aceitos: {string.Join(", ", Nomes)}.");

        Nome = codigo;
    }
}