using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using ZelaFix.Application.Exceptions;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Api.Exceptions;

/// <summary>Converte qualquer exceção não tratada em uma resposta ProblemDetails com o status HTTP adequado.</summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, titulo, detalhe) = Mapear(exception);

        if (status >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Falha em {Metodo} {Caminho}: {Mensagem}",
                httpContext.Request.Method, httpContext.Request.Path, exception.Message);
        else
            logger.LogWarning("Requisição rejeitada ({Status}) em {Metodo} {Caminho}: {Mensagem}",
                status, httpContext.Request.Method, httpContext.Request.Path, exception.Message);

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(
            ErroResposta.Criar(httpContext, status, titulo, detalhe),
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }

    private (int Status, string Titulo, string? Detalhe) Mapear(Exception exception) => exception switch
    {
        DomainException e => (StatusCodes.Status400BadRequest, "Regra de negócio violada", e.Message),
        ConflitoException e => (StatusCodes.Status409Conflict, "Conflito", e.Message),
        AcessoNegadoException e => (StatusCodes.Status403Forbidden, "Acesso negado", e.Message),
        BadHttpRequestException e => (e.StatusCode, "Requisição inválida", e.Message),
        ArgumentException e => (StatusCodes.Status400BadRequest, "Requisição inválida", e.Message),
        KeyNotFoundException e => (StatusCodes.Status404NotFound, "Recurso não encontrado", e.Message),
        UnauthorizedAccessException e => (StatusCodes.Status401Unauthorized, "Não autorizado", e.Message),
        DbUpdateException e => (StatusCodes.Status409Conflict, "Conflito de dados", DetalheBanco(e)),
        OracleException => (StatusCodes.Status502BadGateway, "Banco de dados indisponível",
            "Não foi possível acessar o banco de dados. Tente novamente mais tarde."),
        HttpRequestException => (StatusCodes.Status502BadGateway, "Serviço externo indisponível",
            "Não foi possível consultar um serviço externo necessário. Tente novamente mais tarde."),
        TaskCanceledException { InnerException: TimeoutException } => (StatusCodes.Status504GatewayTimeout,
            "Tempo esgotado", "Um serviço externo demorou demais para responder."),
        TimeoutException => (StatusCodes.Status504GatewayTimeout, "Tempo esgotado",
            "Uma dependência demorou demais para responder."),
        _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor",
            environment.IsDevelopment()
                ? exception.ToString()
                : "Ocorreu um erro inesperado. Tente novamente mais tarde.")
    };

    // Em produção não expõe a mensagem do Oracle; em desenvolvimento ela ajuda a diagnosticar.
    private string DetalheBanco(DbUpdateException e) =>
        environment.IsDevelopment()
            ? e.InnerException?.Message ?? e.Message
            : "A operação viola uma restrição de integridade do banco (por exemplo, o registro está em uso).";
}