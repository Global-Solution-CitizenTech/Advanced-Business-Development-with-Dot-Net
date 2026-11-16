using System.Net.Http.Json;

namespace ZelaFix.Tests.Integration.Fixtures;

internal static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> EnviarComChaveAsync<T>(
        this HttpClient cliente, HttpMethod metodo, string url, T corpo, string chave)
    {
        var requisicao = new HttpRequestMessage(metodo, url) { Content = JsonContent.Create(corpo) };
        requisicao.Headers.Add("Idempotency-Key", chave);
        return cliente.SendAsync(requisicao);
    }
}