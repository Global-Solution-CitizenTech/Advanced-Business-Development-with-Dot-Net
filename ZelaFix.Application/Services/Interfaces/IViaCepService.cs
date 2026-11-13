using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZelaFix.Application.Services.Interfaces;

public sealed record ViaCepResponse(
    [property: JsonPropertyName("cep")] string? Cep,
    [property: JsonPropertyName("logradouro")] string? Logradouro,
    [property: JsonPropertyName("bairro")] string? Bairro,
    [property: JsonPropertyName("localidade")] string? Localidade,
    [property: JsonPropertyName("uf")] string? Uf,
    [property: JsonPropertyName("estado")] string? Estado,
    [property: JsonPropertyName("erro")] JsonElement? Erro)
{
    /// <summary>O ViaCEP sinaliza CEP inexistente com <c>{ "erro": "true" }</c> (ou booleano).</summary>
    [JsonIgnore]
    public bool PossuiErro => Erro is { } e &&
                              (e.ValueKind == JsonValueKind.True ||
                               (e.ValueKind == JsonValueKind.String &&
                                string.Equals(e.GetString(), "true", StringComparison.OrdinalIgnoreCase)));
}

public interface IViaCepService
{
    /// <returns>Os dados do CEP, ou <c>null</c> se ele não existir.</returns>
    /// <exception cref="HttpRequestException">O serviço está indisponível.</exception>
    Task<ViaCepResponse?> ConsultarAsync(string cep, CancellationToken ct = default);
}