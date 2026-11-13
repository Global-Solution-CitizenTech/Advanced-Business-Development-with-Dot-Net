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
    public static ProblemDetails Criar(
        HttpContext http, int status, string titulo, string? mensagem, IReadOnlyList<ErroCampo>? erros = null)
    {
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

        if (erros is { Count: > 0 })
            problem.Extensions["erros"] = erros;

        return problem;
    }
}