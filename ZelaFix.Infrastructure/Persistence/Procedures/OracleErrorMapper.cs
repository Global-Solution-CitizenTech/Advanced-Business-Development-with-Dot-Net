using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;
using ZelaFix.Application.Exceptions;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Infrastructure.Persistence.Procedures;

/// <summary>Traduz os RAISE_APPLICATION_ERROR (-20001..-20012) dos pacotes em exceções da aplicação.</summary>
internal static partial class OracleErroMapper
{
    [GeneratedRegex(@"ORA-(200\d\d):\s*(?<msg>[^\r\n]*)")]
    private static partial Regex Padrao();

    public static Exception Traduzir(OracleException ex)
    {
        var achados = Padrao().Matches(ex.Message);
        if (achados.Count == 0)
            return ex;

        // As procedures de status/abertura reempacotam o erro (-20006/-20007): vale o último código da pilha.
        var ultimo = achados[^1];
        var mensagem = ultimo.Groups["msg"].Value.Trim();

        return int.Parse(ultimo.Groups[1].Value) switch
        {
            20002 => new KeyNotFoundException(mensagem),
            20005 or 20012 => new AcessoNegadoException(mensagem),
            20010 or 20011 => new ConflitoException(mensagem),
            20001 or 20003 or 20004 or 20008 or 20009 => new DomainException(mensagem),
            _ => ex
        };
    }
}