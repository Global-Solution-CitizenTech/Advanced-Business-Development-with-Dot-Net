using ZelaFix.Domain.Common;

namespace ZelaFix.Domain.Entities;

public sealed class Categoria : BaseEntity
{
    public string Nome { get; private set; } = string.Empty;

    private Categoria() { }

    public Categoria(string nome) => Atualizar(nome);

    public void Atualizar(string nome) => Nome = Validacao.Texto(nome, "O nome da categoria", 50);
}