using System.Net;
using System.Net.Http.Json;
using ZelaFix.Application.DTOs;
using ZelaFix.Tests.Integration.Fixtures;

namespace ZelaFix.Tests.Integration.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class HealthIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_SemToken_DeveRetornar200ComOsComponentesVerificados()
    {
        // Arrange / Act
        var response = await _client.GetAsync("/health");
        var corpo = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("oracle-database", corpo);
        Assert.Contains("external-service-viacep", corpo);
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthProbes_SemToken_DeveRetornar200(string rota)
    {
        // Arrange / Act
        var response = await _client.GetAsync(rota);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public class RateLimitingIntegrationTests
{
    [Fact]
    public async Task Login_AposExcederOLimite_DeveRetornar429ComRetryAfter()
    {
        // Arrange
        await using var factory = new RateLimitedWebApplicationFactory();
        var cliente = factory.CreateClient();
        var request = new LoginRequest { Email = "ninguem@teste.com", Senha = "qualquer" };

        // Act
        var primeira = await cliente.PostAsJsonAsync("/api/v1/auth/login", request);
        var segunda = await cliente.PostAsJsonAsync("/api/v1/auth/login", request);
        var terceira = await cliente.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, primeira.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, segunda.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, terceira.StatusCode);
        Assert.NotNull(terceira.Headers.RetryAfter);
    }
}