using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using ZelaFix.Api.Exceptions;
using ZelaFix.Api.Security;

namespace ZelaFix.Api.Idempotency;

/// <summary>
/// Evita operações duplicadas quando o cliente repete a mesma requisição (ex.: timeout de rede).
/// O cliente envia <c>Idempotency-Key</c>; a primeira resposta de sucesso é guardada e reenviada nas repetições.
/// </summary>
public sealed class IdempotencyFilter<TRequest>(
    IIdempotencyStore store,
    IOptions<JsonOptions> jsonOptions,
    ILogger<IdempotencyFilter<TRequest>> logger) : IEndpointFilter where TRequest : class
{
    public const string Cabecalho = "Idempotency-Key";
    public const string CabecalhoRepeticao = "Idempotent-Replayed";
    private const int TamanhoMaximoChave = 100;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        var ct = http.RequestAborted;

        var chaveCliente = http.Request.Headers[Cabecalho].ToString().Trim();
        if (chaveCliente.Length == 0 || chaveCliente.Length > TamanhoMaximoChave)
            return Erro(http, StatusCodes.Status400BadRequest, "Cabeçalho inválido",
                $"Informe o cabeçalho {Cabecalho} (até {TamanhoMaximoChave} caracteres), " +
                "por exemplo um UUID gerado a cada tentativa de envio.");

        var usuario = http.User.ObterUsuarioId()?.ToString() ?? "anonimo";
        var chave = $"{usuario}:{http.Request.Method}:{http.Request.Path}:{chaveCliente}";
        var impressao = CalcularImpressao(context.Arguments.OfType<TRequest>().FirstOrDefault());

        var inicio = await store.IniciarAsync(chave, impressao, ct);
        switch (inicio.Estado)
        {
            case EstadoIdempotencia.PayloadDiferente:
                return Erro(http, StatusCodes.Status422UnprocessableEntity, "Chave de idempotência reutilizada",
                    $"O cabeçalho {Cabecalho} já foi usado com um conteúdo diferente. Gere uma nova chave.");

            case EstadoIdempotencia.EmAndamento:
                return Erro(http, StatusCodes.Status409Conflict, "Requisição em andamento",
                    "Uma requisição com esta chave ainda está sendo processada. Aguarde e tente novamente.");

            case EstadoIdempotencia.Concluido:
                logger.LogInformation("Resposta repetida para a chave de idempotência em {Caminho}.", http.Request.Path);
                http.Response.Headers[CabecalhoRepeticao] = "true";
                return Results.Content(
                    inicio.Resposta!.Corpo, "application/json", Encoding.UTF8, inicio.Resposta.StatusCode);
        }

        try
        {
            var resultado = await next(context);
            await RegistrarAsync(chave, resultado, ct);
            return resultado;
        }
        catch
        {
            // Falhou: libera a chave para o cliente poder tentar de novo.
            await store.LiberarAsync(chave, CancellationToken.None);
            throw;
        }
    }

    private async Task RegistrarAsync(string chave, object? resultado, CancellationToken ct)
    {
        var interno = resultado is INestedHttpResult aninhado ? aninhado.Result : resultado;

        // Só respostas de sucesso com corpo são guardadas; erros (4xx/5xx) liberam a chave.
        if (interno is IStatusCodeHttpResult { StatusCode: >= 200 and < 300 } sucesso
            && interno is IValueHttpResult { Value: { } valor })
        {
            var corpo = JsonSerializer.Serialize(valor, valor.GetType(), jsonOptions.Value.SerializerOptions);
            await store.ConcluirAsync(chave, new RespostaArmazenada(sucesso.StatusCode!.Value, corpo), ct);
        }
        else
        {
            await store.LiberarAsync(chave, ct);
        }
    }

    private string CalcularImpressao(TRequest? request)
    {
        var json = request is null
            ? string.Empty
            : JsonSerializer.Serialize(request, jsonOptions.Value.SerializerOptions);

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
    }

    private static IResult Erro(HttpContext http, int status, string titulo, string mensagem) =>
        Results.Problem(ErroResposta.Criar(http, status, titulo, mensagem));
}

public static class IdempotencyExtensions
{
    /// <summary>Exige o cabeçalho Idempotency-Key. Deve vir DEPOIS de <c>WithValidation</c>.</summary>
    public static RouteHandlerBuilder WithIdempotency<TRequest>(this RouteHandlerBuilder builder) where TRequest : class =>
        builder.AddEndpointFilter<IdempotencyFilter<TRequest>>()
               .ProducesProblem(StatusCodes.Status409Conflict)
               .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}