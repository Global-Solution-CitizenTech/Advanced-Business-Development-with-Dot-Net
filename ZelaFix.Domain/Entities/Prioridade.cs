using ZelaFix.Domain.Common;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Domain.Entities;

public sealed class Prioridade : BaseEntity
{
    public static readonly IReadOnlyList<string> Nomes = ["BAIXA", "MEDIA", "ALTA", "URGENTE"];

    public string Nome { get; private set; } = string.Empty;

    private Prioridade() { }

    public Prioridade(string nome) => Atualizar(nome);

    public void Atualizar(string nome)
    {
        var codigo = (nome ?? string.Empty).Trim().ToUpperInvariant();
        if (!Nomes.Contains(codigo))
            throw new DomainException($"Prioridade inválida. Valores aceitos: {string.Join(", ", Nomes)}.");

        Nome = codigo;
    }
}
