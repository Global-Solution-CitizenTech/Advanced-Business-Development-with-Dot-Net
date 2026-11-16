using ZelaFix.Domain.Common;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Domain.Entities;

public sealed class OrgaoGoverno : BaseEntity
{
    public static readonly IReadOnlyList<string> Nomes =
    [
        "SECRETARIA_OBRAS", "CET_TRANSITO", "ILUMINACAO_PUBLICA",
        "DEFESA_CIVIL", "SUBPREFEITURA", "SANEAMENTO_BASICO"
    ];

    public string Nome { get; private set; } = string.Empty;

    private OrgaoGoverno() { }

    public OrgaoGoverno(string nome) => Atualizar(nome);

    public void Atualizar(string nome)
    {
        var codigo = (nome ?? string.Empty).Trim().ToUpperInvariant();
        if (!Nomes.Contains(codigo))
            throw new DomainException($"Órgão inválido. Valores aceitos: {string.Join(", ", Nomes)}.");

        Nome = codigo;
    }
}
