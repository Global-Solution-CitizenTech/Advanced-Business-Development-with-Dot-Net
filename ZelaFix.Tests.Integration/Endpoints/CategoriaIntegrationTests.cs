using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ZelaFix.Application.DTOs;
using ZelaFix.Tests.Integration.Fixtures;

namespace ZelaFix.Tests.Integration.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class CategoriaIntegrationTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task Listar_SemToken_DeveRetornar401()
    {
        // Arrange
        var cliente = factory.CreateClient();

        // Act
        var response = await cliente.GetAsync("/api/v1/categorias");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Listar_ComCidadao_DeveRetornar200()
    {
        // Arrange
        var (cliente, _) = await factory.CriarCidadaoAsync();

        // Act
        var response = await cliente.GetAsync("/api/v1/categorias");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Criar_ComCidadao_DeveRetornar403()
    {
        // Arrange
        var (cliente, _) = await factory.CriarCidadaoAsync();

        // Act
        var response = await cliente.PostAsJsonAsync("/api/v1/categorias", new CategoriaRequest { Nome = "Poda de Árvore" });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Criar_ComGestor_DeveRetornar201EDuplicadaRetornar409()
    {
        // Arrange
        var gestor = await factory.CriarGestorAsync();
        var request = new CategoriaRequest { Nome = "ILUMINACAO_PUBLICA" };

        // Act
        var primeira = await gestor.PostAsJsonAsync("/api/v1/categorias", request);
        var segunda = await gestor.PostAsJsonAsync("/api/v1/categorias", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, primeira.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);
    }

    [Fact]
    public async Task Criar_ComNomeCurto_DeveRetornar400ComErroPorCampo()
    {
        // Arrange
        var gestor = await factory.CriarGestorAsync();

        // Act
        var response = await gestor.PostAsJsonAsync("/api/v1/categorias", new CategoriaRequest { Nome = "A" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var corpo = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("nome", corpo.GetProperty("erros")[0].GetProperty("campo").GetString());
    }

    [Fact]
    public async Task Criar_ComNomeForaDoCatalogo_DeveRetornar400()
    {
        var gestor = await factory.CriarGestorAsync();

        var response = await gestor.PostAsJsonAsync(
            "/api/v1/categorias", new CategoriaRequest { Nome = "Categoria personalizada" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}