using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ZelaFix.Infrastructure.Persistence;

namespace ZelaFix.Api.Health;

/// <summary>Verifica a conectividade com o Oracle pelo próprio EF Core (sem pacote extra).</summary>
public sealed class OracleDbHealthCheck(ZelaFixDbContext context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthCheckContext, CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Conexão com o Oracle estabelecida com sucesso.")
                : HealthCheckResult.Unhealthy("Não foi possível conectar ao Oracle.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Falha ao verificar a conectividade com o Oracle.", ex);
        }
    }
}