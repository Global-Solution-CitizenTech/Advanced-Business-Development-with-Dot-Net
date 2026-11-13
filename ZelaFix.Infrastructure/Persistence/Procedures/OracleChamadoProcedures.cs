using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using ZelaFix.Application.Repositories;

namespace ZelaFix.Infrastructure.Persistence.Procedures;

public sealed class OracleChamadoProcedures(ZelaFixDbContext context) : IChamadoProcedures
{
    public async Task<int> AbrirChamadoAsync(string titulo, string descricao, byte[] foto,
        int usuarioId, int enderecoId, int categoriaId, CancellationToken ct = default)
    {
        var id = new OracleParameter("p_id_chamado_gerado", OracleDbType.Decimal, ParameterDirection.Output);
        await ExecutarProcedureAsync("pkg_chamado_gestao.pr_abrir_chamado", ct,
            Entrada("p_titulo", OracleDbType.Varchar2, titulo),
            Entrada("p_descricao", OracleDbType.Varchar2, descricao),
            Entrada("p_foto", OracleDbType.Blob, foto),
            Entrada("p_id_usuario", OracleDbType.Int32, usuarioId),
            Entrada("p_id_endereco", OracleDbType.Int32, enderecoId),
            Entrada("p_id_categoria", OracleDbType.Int32, categoriaId),
            id);
        return LerInteiro(id);
    }

    public Task AtualizarStatusAsync(int chamadoId, int statusId, int fiscalId, string observacoes, CancellationToken ct = default) =>
        ExecutarProcedureAsync("pkg_chamado_gestao.pr_atualizar_status_chamado", ct,
            Entrada("p_id_chamado", OracleDbType.Int32, chamadoId),
            Entrada("p_id_status", OracleDbType.Int32, statusId),
            Entrada("p_id_usuario_fiscal", OracleDbType.Int32, fiscalId),
            Entrada("p_observacoes", OracleDbType.Varchar2, observacoes));

    public async Task<int> RegistrarVotoAsync(int chamadoId, int usuarioId, CancellationToken ct = default)
    {
        var id = new OracleParameter("p_id_voto_gerado", OracleDbType.Decimal, ParameterDirection.Output);
        await ExecutarProcedureAsync("pkg_engajamento_cidadao.pr_registrar_voto", ct,
            Entrada("p_id_chamado", OracleDbType.Int32, chamadoId),
            Entrada("p_id_usuario", OracleDbType.Int32, usuarioId),
            id);
        return LerInteiro(id);
    }

    public async Task<string> ClassificarCriticidadeBairroAsync(int bairroId, CancellationToken ct = default)
    {
        await context.Database.OpenConnectionAsync(ct);
        await using var cmd = CriarComando(
            "SELECT pkg_engajamento_cidadao.fn_classificar_criticidade_bairro(:p_id_bairro) FROM DUAL",
            CommandType.Text, [Entrada("p_id_bairro", OracleDbType.Int32, bairroId)]);
        try
        {
            return Convert.ToString(await cmd.ExecuteScalarAsync(ct)) ?? string.Empty;
        }
        catch (OracleException ex)
        {
            throw OracleErroMapper.Traduzir(ex);
        }
    }

    public async Task<string> RelatorioEngajamentoJsonAsync(int bairroId, CancellationToken ct = default)
    {
        var json = new OracleParameter("p_relatorio_json", OracleDbType.Clob, ParameterDirection.Output);
        await ExecutarProcedureAsync("pkg_engajamento_cidadao.pr_relatorio_engajamento_json", ct,
            Entrada("p_id_bairro", OracleDbType.Int32, bairroId), json);

        if (json.Value is OracleClob { IsNull: false } clob)
        {
            using (clob)
                return clob.Value;
        }

        return json.Value?.ToString() ?? string.Empty;
    }

    private async Task ExecutarProcedureAsync(string procedure, CancellationToken ct, params OracleParameter[] parametros)
    {
        await context.Database.OpenConnectionAsync(ct);
        await using var cmd = CriarComando(procedure, CommandType.StoredProcedure, parametros);
        try
        {
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch (OracleException ex)
        {
            throw OracleErroMapper.Traduzir(ex);
        }
    }

    // Usa a conexão e a transação do EF: o que a procedure grava entra no mesmo commit/rollback da requisição.
    private DbCommand CriarComando(string texto, CommandType tipo, IEnumerable<OracleParameter> parametros)
    {
        var cmd = context.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = texto;
        cmd.CommandType = tipo;
        cmd.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
        if (cmd is OracleCommand oracle)
            oracle.BindByName = true;

        foreach (var p in parametros)
            cmd.Parameters.Add(p);

        return cmd;
    }

    private static OracleParameter Entrada(string nome, OracleDbType tipo, object valor) =>
        new(nome, tipo, valor, ParameterDirection.Input);

    private static int LerInteiro(OracleParameter parametro) => parametro.Value switch
    {
        OracleDecimal d => Convert.ToInt32(d.Value),
        var v => Convert.ToInt32(v)
    };
}