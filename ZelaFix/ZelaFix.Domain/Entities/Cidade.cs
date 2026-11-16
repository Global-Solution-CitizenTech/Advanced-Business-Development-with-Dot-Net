using ZelaFix.Domain.Common;

namespace ZelaFix.Domain.Entities;

public sealed class Cidade : BaseEntity
{
    public string Nome { get; private set; } = string.Empty;
    public int EstadoId { get; private set; }
    public Estado Estado { get; private set; } = null!;
    public List<Bairro> Bairros { get; private set; } = [];

    private Cidade() { }

    public Cidade(string nome, int estadoId) => Atualizar(nome, estadoId);

    public void Atualizar(string nome, int estadoId)
    {
        Nome = Validacao.Texto(nome, "O nome da cidade", 50);
        EstadoId = Validacao.Id(estadoId, "A cidade deve estar associada a um estado válido.");
    }
}