using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ZelaFix.Api.Health;

namespace ZelaFix.Api.Extensions;

public static class HealthExtensions
{
    public static IServiceCollection AddZelaFixHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("Processo da API ativo e operacional."), tags: ["live"])
            .AddCheck<OracleDbHealthCheck>("oracle-database", tags: ["ready", "db"])
            .AddCheck<ViaCepHealthCheck>("external-service-viacep", tags: ["ready", "external"]);

        return services;
    }

    public static IEndpointRouteBuilder MapZelaFixHealthChecks(this IEndpointRouteBuilder app)
    {
        // Visão geral: API + banco + serviço externo.
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
        }).AllowAnonymous();

        // Readiness: só o que a API precisa para atender (banco e ViaCEP).
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
        }).AllowAnonymous();

        // Liveness: o processo está de pé, sem dependências externas.
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
        }).AllowAnonymous();

        return app;
    }
}