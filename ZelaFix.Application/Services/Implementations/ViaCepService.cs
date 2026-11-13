using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using ZelaFix.Application.Services.Interfaces;

namespace ZelaFix.Application.Services.Implementations;

public sealed class ViaCepService(HttpClient httpClient, ILogger<ViaCepService> logger) : IViaCepService
{
    public async Task<ViaCepResponse?> ConsultarAsync(string cep, CancellationToken ct = default)
    {
        logger.LogInformation("Consultando o ViaCEP para o CEP {Cep}.", cep);

        try
        {
            var resposta = await httpClient.GetFromJsonAsync<ViaCepResponse>($"ws/{cep}/json/", ct);
            if (resposta is null || resposta.PossuiErro)
            {
                logger.LogWarning("O ViaCEP não encontrou o CEP {Cep}.", cep);
                return null;
            }

            return resposta;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            logger.LogWarning("O ViaCEP rejeitou o CEP {Cep} por formato inválido.", cep);
            return null;
        }
    }
}