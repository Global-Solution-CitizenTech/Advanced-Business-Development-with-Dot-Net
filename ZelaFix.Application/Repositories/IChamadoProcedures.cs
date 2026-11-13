namespace ZelaFix.Application.Repositories;

/// <summary>Chamadas aos pacotes PL/SQL do banco (pkg_chamado_gestao e pkg_engajamento_cidadao).</summary>
public interface IChamadoProcedures
{
    Task<int> AbrirChamadoAsync(string titulo, string descricao, byte[] foto,
        int usuarioId, int enderecoId, int categoriaId, CancellationToken ct = default);

    Task AtualizarStatusAsync(int chamadoId, int statusId, int fiscalId, string observacoes, CancellationToken ct = default);

    Task<int> RegistrarVotoAsync(int chamadoId, int usuarioId, CancellationToken ct = default);

    Task<string> ClassificarCriticidadeBairroAsync(int bairroId, CancellationToken ct = default);

    Task<string> RelatorioEngajamentoJsonAsync(int bairroId, CancellationToken ct = default);
}