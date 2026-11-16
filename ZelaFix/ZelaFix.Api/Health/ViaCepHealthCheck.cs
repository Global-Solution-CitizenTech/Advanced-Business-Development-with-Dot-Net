using Microsoft.Extensions.Diagnostics.HealthChecks;
using ZelaFix.Application.Services.Interfaces;

namespace ZelaFix.Api.Health;

/// <summary>Verifica o ViaCEP, usado no cadastro e na abertura de chamados. Indisponível = Degraded (a API segue de pé).</summary>
public sealed class ViaCepHealthCheck(IViaCepService viaCep) : IHealthCheck
{
    private const string CepDeTeste = "01001000";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return await viaCep.ConsultarAsync(CepDeTeste, cancellationToken) is not null
                ? HealthCheckResult.Healthy("Serviço ViaCEP respondendo normalmente.")
                : HealthCheckResult.Degraded("O ViaCEP respondeu, mas não encontrou o CEP de teste.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("Serviço ViaCEP indisponível no momento.", ex);
        }
    }
}