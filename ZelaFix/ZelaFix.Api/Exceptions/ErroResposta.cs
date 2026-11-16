using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ZelaFix.Api.Exceptions;

public sealed record ErroCampo(string Campo, string Mensagem);

/// <summary>
/// Monta o corpo de erro padrão da API: RFC 7807 (ProblemDetails) mais os campos que o app Mobile
/// já entende (error, message, path, timestamp e erros[{ campo, mensagem }]).
/// </summary>
public static class ErroResposta
{
    public const string CorrelationHeader = "X-Correlation-ID";

    public static ProblemDetails Criar(
        HttpContext http, int status, string titulo, string? mensagem, IReadOnlyList<ErroCampo>? erros = null)
    {
        var correlationId = http.Request.Headers.TryGetValue(CorrelationHeader, out var reqCid) && !string.IsNullOrWhiteSpace(reqCid)
            ? reqCid.ToString()
            : (http.Response.Headers.TryGetValue(CorrelationHeader, out var resCid) && !string.IsNullOrWhiteSpace(resCid)
                ? resCid.ToString()
                : (http.Items.TryGetValue(CorrelationHeader, out var itemCid) && itemCid is not null
                    ? itemCid.ToString()
                    : http.TraceIdentifier));

        var problem = new ProblemDetails
        {
            Type = "about:blank",
            Title = titulo,
            Status = status,
            Detail = mensagem,
            Instance = http.Request.Path
        };

        problem.Extensions["error"] = titulo;
        problem.Extensions["message"] = mensagem ?? titulo;
        problem.Extensions["path"] = http.Request.Path.Value;
        problem.Extensions["timestamp"] = DateTime.UtcNow;
        problem.Extensions["traceId"] = Activity.Current?.Id ?? http.TraceIdentifier;
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            problem.Extensions["correlationId"] = correlationId;
        }

        if (erros is { Count: > 0 })
            problem.Extensions["erros"] = erros;

        return problem;
    }
}
