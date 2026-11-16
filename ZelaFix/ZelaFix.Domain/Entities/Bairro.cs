using ZelaFix.Domain.Common;

namespace ZelaFix.Domain.Entities;

public sealed class Bairro : BaseEntity
{
    public string Nome { get; private set; } = string.Empty;
    public int CidadeId { get; private set; }
    public Cidade Cidade { get; private set; } = null!;

    private Bairro() { }

    public Bairro(string nome, int cidadeId) => Atualizar(nome, cidadeId);

    public void Atualizar(string nome, int cidadeId)
    {
        Nome = Validacao.Texto(nome, "O nome do bairro", 50);
        CidadeId = Validacao.Id(cidadeId, "O bairro deve estar associado a uma cidade válida.");
    }
}