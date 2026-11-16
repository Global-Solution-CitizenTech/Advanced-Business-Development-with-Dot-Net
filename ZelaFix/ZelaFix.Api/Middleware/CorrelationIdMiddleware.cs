using Serilog.Context;

namespace ZelaFix.Api.Middleware;

/// <summary>
/// Extrai ou gera o Correlation ID da requisição (header X-Correlation-ID), injeta no contexto
/// estruturado de logs (Serilog LogContext e ILogger Scope) e inclui o header na resposta HTTP.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string CorrelationHeader = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var correlationId = httpContext.Request.Headers.TryGetValue(CorrelationHeader, out var headerVal) && !string.IsNullOrWhiteSpace(headerVal)
            ? headerVal.ToString()
            : (string.IsNullOrWhiteSpace(httpContext.TraceIdentifier) ? Guid.NewGuid().ToString("D") : httpContext.TraceIdentifier);

        httpContext.Items[CorrelationHeader] = correlationId;

        if (!httpContext.Response.Headers.ContainsKey(CorrelationHeader))
        {
            httpContext.Response.Headers[CorrelationHeader] = correlationId;
        }

        httpContext.Response.OnStarting(() =>
        {
            httpContext.Response.Headers[CorrelationHeader] = correlationId;
            return Task.CompletedTask;
        });

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(httpContext);
        }
    }
}
