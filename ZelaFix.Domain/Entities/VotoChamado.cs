using ZelaFix.Domain.Common;

namespace ZelaFix.Domain.Entities;

public sealed class VotoChamado : BaseEntity
{
    public DateTime DataVoto { get; private set; }

    public int ChamadoId { get; private set; }
    public Chamado Chamado { get; private set; } = null!;

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;

    private VotoChamado() { }

    public VotoChamado(int chamadoId, int usuarioId)
    {
        ChamadoId = Validacao.Id(chamadoId, "O voto deve pertencer a um chamado válido.");
        UsuarioId = Validacao.Id(usuarioId, "O voto deve ter um autor válido.");
        DataVoto = DateTime.UtcNow;
    }
}