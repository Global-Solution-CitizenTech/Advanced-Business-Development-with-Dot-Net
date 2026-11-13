using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using ZelaFix.Api.Exceptions;

namespace ZelaFix.Api.Extensions;

public sealed class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    public int GlobalPermitLimit { get; set; } = 100;
    public int AuthPermitLimit { get; set; } = 10;
    public int WindowSeconds { get; set; } = 60;
}

public static class RateLimitingExtensions
{
    public const string AuthPolicy = "auth";

    public static IServiceCollection AddZelaFixRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(RateLimitingSettings.SectionName).Get<RateLimitingSettings>()
                       ?? new RateLimitingSettings();
        var janela = TimeSpan.FromSeconds(settings.WindowSeconds);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Limite global por IP. Os health checks ficam de fora (orquestradores consultam com frequência).
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
                http.Request.Path.StartsWithSegments("/health")
                    ? RateLimitPartition.GetNoLimiter("health")
                    : RateLimitPartition.GetFixedWindowLimiter(Cliente(http), _ => Janela(settings.GlobalPermitLimit, janela)));

            // Política mais rígida para login e cadastro (mitiga força bruta).
            options.AddPolicy(AuthPolicy, http =>
                RateLimitPartition.GetFixedWindowLimiter(Cliente(http), _ => Janela(settings.AuthPermitLimit, janela)));

            options.OnRejected = async (contexto, ct) =>
            {
                var http = contexto.HttpContext;
                var segundos = contexto.Lease.TryGetMetadata(MetadataName.RetryAfter, out var espera)
                    ? (int)Math.Ceiling(espera.TotalSeconds)
                    : settings.WindowSeconds;

                http.Response.Headers.RetryAfter = segundos.ToString();
                http.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await http.Response.WriteAsJsonAsync(
                    ErroResposta.Criar(http, StatusCodes.Status429TooManyRequests, "Muitas requisições",
                        $"Limite de requisições excedido. Tente novamente em {segundos} segundos."),
                    options: null, contentType: "application/problem+json", cancellationToken: ct);
            };
        });

        return services;
    }

    private static string Cliente(HttpContext http) => http.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

    private static FixedWindowRateLimiterOptions Janela(int limite, TimeSpan janela) => new()
    {
        PermitLimit = limite,
        Window = janela,
        QueueLimit = 0
    };
}