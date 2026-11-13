using ZelaFix.Domain.Common;

namespace ZelaFix.Domain.Entities;

public sealed class Estado : BaseEntity
{
    public string Nome { get; private set; } = string.Empty;
    public List<Cidade> Cidades { get; private set; } = [];

    private Estado() { }

    public Estado(string nome) => Atualizar(nome);

    public void Atualizar(string nome) => Nome = Validacao.Texto(nome, "O nome do estado", 30);
}