using ZelaFix.Domain.Common;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Domain.Entities;

public sealed class Chamado : BaseEntity
{
    public string Titulo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public byte[] Foto { get; private set; } = [];
    public DateTime DataAbertura { get; private set; }

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;

    public int EnderecoId { get; private set; }
    public Endereco Endereco { get; private set; } = null!;

    public int CategoriaId { get; private set; }
    public Categoria Categoria { get; private set; } = null!;

    public int PrioridadeId { get; private set; }
    public Prioridade Prioridade { get; private set; } = null!;

    public int OrgaoGovernoId { get; private set; }
    public OrgaoGoverno OrgaoGoverno { get; private set; } = null!;

    public List<HistChamado> Historico { get; private set; } = [];
    public List<VotoChamado> Votos { get; private set; } = [];

    /// <summary>A tabela CHAMADO não guarda status: vale o do último registro do histórico.</summary>
    public Status? StatusAtual => Historico
        .OrderByDescending(h => h.DataMudanca)
        .ThenByDescending(h => h.Id)
        .FirstOrDefault()?.Status;

    private Chamado() { }

    public Chamado(string titulo, string descricao, byte[] foto, int usuarioId, int enderecoId,
        int categoriaId, int prioridadeId, int orgaoGovernoId)
    {
        Atualizar(titulo, descricao, foto, enderecoId, categoriaId);
        UsuarioId = Validacao.Id(usuarioId, "O chamado deve ter um autor válido.");
        PrioridadeId = Validacao.Id(prioridadeId, "O chamado deve ter uma prioridade válida.");
        OrgaoGovernoId = Validacao.Id(orgaoGovernoId, "O chamado deve ter um órgão responsável válido.");
        DataAbertura = DateTime.UtcNow;
    }

    public void Atualizar(string titulo, string descricao, byte[] foto, int enderecoId, int categoriaId)
    {
        Titulo = Validacao.Texto(titulo, "O título", 100);
        Descricao = Validacao.Texto(descricao, "A descrição", 1000);

        if (foto is null || foto.Length == 0)
            throw new DomainException("A foto do chamado é obrigatória.");
        Foto = foto;

        EnderecoId = Validacao.Id(enderecoId, "O chamado deve ter um endereço válido.");
        CategoriaId = Validacao.Id(categoriaId, "O chamado deve ter uma categoria válida.");
    }

    public void AtualizarPrioridade(int prioridadeId)
    {
        PrioridadeId = Validacao.Id(prioridadeId, "O chamado deve ter uma prioridade válida.");
    }

    public void RegistrarHistorico(int statusId, int usuarioId, string observacoes) =>
        Historico.Add(new HistChamado(statusId, usuarioId, observacoes));

    public bool JaApoiadoPor(int usuarioId) => Votos.Any(v => v.UsuarioId == usuarioId);

    public void Apoiar(int usuarioId)
    {
        if (JaApoiadoPor(usuarioId))
            throw new DomainException("Você já apoiou este chamado.");

        Votos.Add(new VotoChamado(Id, usuarioId));
    }
}