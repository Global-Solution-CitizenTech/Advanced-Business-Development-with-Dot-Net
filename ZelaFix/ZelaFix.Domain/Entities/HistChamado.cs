using ZelaFix.Domain.Common;

namespace ZelaFix.Domain.Entities;

public sealed class HistChamado : BaseEntity
{
    public DateTime DataMudanca { get; private set; }
    public string Observacoes { get; private set; } = string.Empty;

    public int ChamadoId { get; private set; }
    public Chamado Chamado { get; private set; } = null!;

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;

    public int StatusId { get; private set; }
    public Status Status { get; private set; } = null!;

    private HistChamado() { }

    /// <summary>Usado ao registrar o histórico pela navegação de <see cref="Chamado"/>.</summary>
    public HistChamado(int statusId, int usuarioId, string observacoes)
    {
        StatusId = Validacao.Id(statusId, "O status é obrigatório.");
        UsuarioId = Validacao.Id(usuarioId, "O autor da mudança é obrigatório.");
        Observacoes = Validacao.Texto(observacoes, "A observação", 255);
        DataMudanca = DateTime.UtcNow;
    }

    /// <summary>Usado para acrescentar uma mudança a um chamado já existente.</summary>
    public HistChamado(int chamadoId, int statusId, int usuarioId, string observacoes)
        : this(statusId, usuarioId, observacoes)
    {
        ChamadoId = Validacao.Id(chamadoId, "O histórico deve pertencer a um chamado válido.");
    }
}