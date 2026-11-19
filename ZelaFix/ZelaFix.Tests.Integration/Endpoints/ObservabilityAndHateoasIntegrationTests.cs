using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ZelaFix.Application.DTOs;
using ZelaFix.Tests.Integration.Fixtures;

namespace ZelaFix.Tests.Integration.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class ObservabilityAndHateoasIntegrationTests(CustomWebApplicationFactory factory)
{
    private const string PngBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";

    [Fact]
    public async Task Request_SemHeaderCorrelationId_DeveGerarERetornarCorrelationIdNoHeader()
    {
        // Arrange
        var cliente = factory.CreateClient();

        // Act
        var response = await cliente.GetAsync("/health");

        // Assert
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        var correlationId = response.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
    }

    [Fact]
    public async Task Request_ComHeaderCorrelationId_DevePropagarMesmoCorrelationIdNoHeader()
    {
        // Arrange
        var cliente = factory.CreateClient();
        var customId = "zelafix-corr-" + Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Correlation-ID", customId);

        // Act
        var response = await cliente.SendAsync(request);

        // Assert
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        var correlationId = response.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
        Assert.Equal(customId, correlationId);
    }

    [Fact]
    public async Task RespostaErro_DeveSeguirProblemDetailsRFC7807EConterCorrelationId()
    {
        // Arrange
        var cliente = factory.CreateClient();
        var customId = "zelafix-error-corr-" + Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/categorias");
        request.Headers.Add("X-Correlation-ID", customId);

        // Act
        var response = await cliente.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("about:blank", doc.GetProperty("type").GetString());
        Assert.Equal(401, doc.GetProperty("status").GetInt32());
        Assert.True(doc.TryGetProperty("correlationId", out var cidProp));
        Assert.Equal(customId, cidProp.GetString());
    }

    [Fact]
    public async Task ListarCategorias_DeveRetornarLinksHateoas()
    {
        // Arrange
        var (cliente, _) = await factory.CriarCidadaoAsync();

        // Act
        var response = await cliente.GetAsync("/api/v1/categorias");

        // Assert
        response.EnsureSuccessStatusCode();
        var categorias = await response.Content.ReadFromJsonAsync<List<CategoriaResponse>>();
        Assert.NotNull(categorias);
        Assert.NotEmpty(categorias);

        var primeira = categorias[0];
        Assert.NotNull(primeira.Links);
        Assert.Contains(primeira.Links, l => l.Rel == "self" && l.Method == "GET");
        Assert.Contains(primeira.Links, l => l.Rel == "collection" && l.Method == "GET");
        Assert.Contains(primeira.Links, l => l.Rel == "chamados" && l.Method == "GET");
    }

    [Fact]
    public async Task Chamados_FluxoComHateoas_DeveRetornarLinksNoChamadoENaPagina()
    {
        // Arrange
        var (cliente, _) = await factory.CriarCidadaoAsync();
        var chave = Guid.NewGuid().ToString();
        var createRequest = new CriarChamadoRequest
        {
            Titulo = "Luz queimada na praça",
            Descricao = "Poste sem iluminação trazendo insegurança à noite.",
            CategoriaId = factory.CategoriaId,
            Cep = "01001-000",
            Numero = "123",
            FotoBase64 = PngBase64
        };

        // Act - Criar
        var createResp = await cliente.EnviarComChaveAsync(HttpMethod.Post, "/api/v1/chamados", createRequest, chave);
        createResp.EnsureSuccessStatusCode();
        var chamadoCriado = await createResp.Content.ReadFromJsonAsync<ChamadoResponse>();

        // Assert - Chamado Criado tem links
        Assert.NotNull(chamadoCriado);
        Assert.NotNull(chamadoCriado.Links);
        Assert.Contains(chamadoCriado.Links, l => l.Rel == "self" && l.Href == $"/api/v1/chamados/{chamadoCriado.Id}");
        Assert.Contains(chamadoCriado.Links, l => l.Rel == "apoiar" && l.Method == "POST");
        Assert.Contains(chamadoCriado.Links, l => l.Rel == "alterar-status" && l.Method == "PATCH");

        // Act - Listar com paginação
        var listResp = await cliente.GetAsync("/api/v1/chamados?apenasMeus=true");
        listResp.EnsureSuccessStatusCode();
        var pagina = await listResp.Content.ReadFromJsonAsync<PaginaResponse<ChamadoResponse>>();

        // Assert - Pagina tem links e item tem links
        Assert.NotNull(pagina);
        Assert.NotNull(pagina.Links);
        Assert.Contains(pagina.Links, l => l.Rel == "self");
        Assert.Contains(pagina.Links, l => l.Rel == "first");
        Assert.NotEmpty(pagina.Content);
        Assert.NotNull(pagina.Content[0].Links);
    }

    [Fact]
    public async Task SwaggerJson_DeveExporTodosEndpointsIdempotencyHeaderE429()
    {
        // Arrange
        var cliente = factory.CreateClient();

        // Act
        var response = await cliente.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.EnsureSuccessStatusCode();
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
        var paths = doc.GetProperty("paths");

        Assert.True(paths.TryGetProperty("/health", out _));
        Assert.True(paths.TryGetProperty("/login", out _));
        Assert.True(paths.TryGetProperty("/usuarios/me", out _));
        Assert.True(paths.TryGetProperty("/api/v1/chamados", out var chamadosPath));

        var postChamados = chamadosPath.GetProperty("post");
        var headerParams = postChamados.GetProperty("parameters").EnumerateArray().ToList();
        Assert.Contains(headerParams, p =>
            p.GetProperty("name").GetString() == "Idempotency-Key" &&
            p.GetProperty("in").GetString() == "header" &&
            p.GetProperty("required").GetBoolean());

        var responses = postChamados.GetProperty("responses");
        Assert.True(responses.TryGetProperty("429", out _));
        Assert.True(chamadosPath.GetProperty("get").GetProperty("responses").TryGetProperty("400", out _));
        Assert.Equal(23, paths.EnumerateObject().Count());
        Assert.True(doc.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("Bearer", out _));
    }
}
