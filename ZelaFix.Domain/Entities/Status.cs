using System.Globalization;
using System.Text;
using ZelaFix.Domain.Common;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Domain.Entities;

public sealed class Status : BaseEntity
{
    // Valores do CHECK ck_status_nome do banco.
    public const string Aberto = "ABERTO";
    public const string EmAnalise = "EM_ANALISE";
    public const string EmExecucao = "EM_EXECUCAO";
    public const string Concluido = "CONCLUIDO";
    public const string Rejeitado = "REJEITADO";

    public static readonly IReadOnlyList<string> Todos = [Aberto, EmAnalise, EmExecucao, Concluido, Rejeitado];

    public string Nome { get; private set; } = string.Empty;

    private Status() { }

    public Status(string nome) => Atualizar(nome);

    public void Atualizar(string nome) => Nome = Canonico(nome);

    /// <summary>Aceita "Em Análise", "em_analise", "EM ANALISE"... e devolve o código oficial, ou lança.</summary>
    public static string Canonico(string? nome)
    {
        var texto = (nome ?? string.Empty).Trim().Normalize(NormalizationForm.FormD);
        var semAcento = new string(texto
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
        var codigo = semAcento.ToUpperInvariant().Replace(' ', '_').Replace('-', '_');

        return Todos.Contains(codigo)
            ? codigo
            : throw new DomainException($"Status inválido. Valores aceitos: {string.Join(", ", Todos)}.");
    }
}