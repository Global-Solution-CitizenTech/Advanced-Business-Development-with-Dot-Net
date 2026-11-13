using System.Net;
using System.Net.Http.Json;
using ZelaFix.Application.DTOs;
using ZelaFix.Tests.Integration.Fixtures;

namespace ZelaFix.Tests.Integration.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class ChamadoIntegrationTests(CustomWebApplicationFactory factory)
{
    private const string PngBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";

    private CriarChamadoRequest Novo(string? titulo = null) => new()
    {
        Titulo = titulo ?? $"Buraco {Guid.NewGuid():N}"[..20],
        Descricao = "Buraco grande na calçada, risco para pedestres.",
        CategoriaId = factory.CategoriaId,
        Cep = "01001-000",
        Numero = "10",
        FotoBase64 = PngBase64
    };

    private static string NovaChave() => Guid.NewGuid().ToString();

    private async Task<ChamadoResponse> AbrirAsync(HttpClient cliente, CriarChamadoRequest? request = null)
    {
        var resposta = await cliente.EnviarComChaveAsync(HttpMethod.Post, "/api/v1/chamados", request ?? Novo(), NovaChave());
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<ChamadoResponse>())!;
    }

    private static Task<HttpResponseMessage> MudarStatusAsync(HttpClient gestor, int id, string status) =>
        gestor.EnviarComChaveAsync(new HttpMethod("PATCH"), $"/api/v1/chamados/{id}/status",
            new AlterarStatusRequest { Status = status }, NovaChave());

    [Fact]
    public async Task Criar_SemIdempotencyKey_DeveRetornar400()
    {
        // Arrange
        var (cliente, _) = await factory.CriarCidadaoAsync();

        // Act
        var response = await cliente.PostAsJsonAsync("/api/v1/chamados", Novo());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Criar_ComDadosValidos_DeveRetornar201ComStatusAbertoEFotoEmBase64()
    {
        // Arrange
        var (cliente, _) = await factory.CriarCidadaoAsync();

        // Act
        var chamado = await AbrirAsync(cliente);

        // Assert
        Assert.Equal("ABERTO", chamado.Status);
        Assert.Equal(0, chamado.ApoiosCount);
        Assert.StartsWith("ZEL-", chamado.Protocolo);
        Assert.Equal("image/png", chamado.FotoContentType);
        Assert.Equal(PngBase64, chamado.FotoBase64);
        Assert.Single(chamado.Historico!);
    }

    [Fact]
    public async Task Criar_RepetindoMesmaChave_DeveReenviarRespostaSemDuplicarOChamado()
    {
        // Arrange
        var (cliente, _) = await factory.CriarCidadaoAsync();
        var request = Novo();
        var chave = NovaChave();

        // Act
        var primeira = await cliente.EnviarComChaveAsync(HttpMethod.Post, "/api/v1/chamados", request, chave);
        var segunda = await cliente.EnviarComChaveAsync(HttpMethod.Post, "/api/v1/chamados", request, chave);

        // Assert
        Assert.Equal(HttpStatusCode.Created, primeira.StatusCode);
        Assert.Equal(HttpStatusCode.Created, segunda.StatusCode);
        Assert.True(segunda.Headers.Contains("Idempotent-Replayed"));

        var original = await primeira.Content.ReadFromJsonAsync<ChamadoResponse>();
        var repetido = await segunda.Content.ReadFromJsonAsync<ChamadoResponse>();
        Assert.Equal(original!.Id, repetido!.Id);

        var lista = await cliente.GetFromJsonAsync<PaginaResponse<ChamadoResponse>>(
            $"/api/v1/chamados?busca={Uri.EscapeDataString(request.Titulo)}");
        Assert.Equal(1, lista!.TotalElements);
    }

    [Fact]
    public async Task Criar_MesmaChaveComCorpoDiferente_DeveRetornar422()
    {
        // Arrange
        var (cliente, _) = await factory.CriarCidadaoAsync();
        var chave = NovaChave();
        await cliente.EnviarComChaveAsync(HttpMethod.Post, "/api/v1/chamados", Novo("Titulo original"), chave);

        // Act
        var response = await cliente.EnviarComChaveAsync(HttpMethod.Post, "/api/v1/chamados", Novo("Titulo diferente"), chave);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Apoiar_PeloProprioAutor_DeveRetornar403()
    {
        // Arrange
        var (autor, _) = await factory.CriarCidadaoAsync();
        var chamado = await AbrirAsync(autor);

        // Act
        var response = await autor.PostAsync($"/api/v1/chamados/{chamado.Id}/apoiar", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Apoiar_PorOutroUsuarioDuasVezes_DeveContarUmUnicoApoio()
    {
        // Arrange
        var (autor, _) = await factory.CriarCidadaoAsync();
        var (outro, _) = await factory.CriarCidadaoAsync();
        var chamado = await AbrirAsync(autor);

        // Act
        await outro.PostAsync($"/api/v1/chamados/{chamado.Id}/apoiar", null);
        var segunda = await outro.PostAsync($"/api/v1/chamados/{chamado.Id}/apoiar", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, segunda.StatusCode);
        var atualizado = await segunda.Content.ReadFromJsonAsync<ChamadoResponse>();
        Assert.Equal(1, atualizado!.ApoiosCount);
        Assert.True(atualizado.ApoiadoPeloUsuario);
    }

    [Fact]
    public async Task AlterarStatus_ComCidadao_DeveRetornar403()
    {
        // Arrange
        var (cidadao, _) = await factory.CriarCidadaoAsync();
        var chamado = await AbrirAsync(cidadao);

        // Act
        var response = await MudarStatusAsync(cidadao, chamado.Id, "EM_ANALISE");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AlterarStatus_ComGestor_DeveRetornar200EAcrescentarHistorico()
    {
        // Arrange
        var (cidadao, _) = await factory.CriarCidadaoAsync();
        var gestor = await factory.CriarGestorAsync();
        var chamado = await AbrirAsync(cidadao);

        // Act
        var response = await MudarStatusAsync(gestor, chamado.Id, "Em Análise");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var atualizado = await response.Content.ReadFromJsonAsync<ChamadoResponse>();
        Assert.Equal("EM_ANALISE", atualizado!.Status);
        Assert.Equal(2, atualizado.Historico!.Count);
        Assert.Equal("ABERTO", atualizado.Historico[1].StatusAnterior);
    }

    [Fact]
    public async Task AlterarStatus_ComValorInvalido_DeveRetornar400()
    {
        // Arrange
        var (cidadao, _) = await factory.CriarCidadaoAsync();
        var gestor = await factory.CriarGestorAsync();
        var chamado = await AbrirAsync(cidadao);

        // Act
        var response = await MudarStatusAsync(gestor, chamado.Id, "Resolvido");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Apoiar_ChamadoConcluido_DeveRetornar409()
    {
        // Arrange
        var (autor, _) = await factory.CriarCidadaoAsync();
        var (outro, _) = await factory.CriarCidadaoAsync();
        var gestor = await factory.CriarGestorAsync();
        var chamado = await AbrirAsync(autor);
        await MudarStatusAsync(gestor, chamado.Id, "CONCLUIDO");

        // Act
        var response = await outro.PostAsync($"/api/v1/chamados/{chamado.Id}/apoiar", null);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Listar_FiltrandoPorStatus_DeveRetornarSomenteOsDoStatus()
    {
        // Arrange
        var (cidadao, _) = await factory.CriarCidadaoAsync();
        var gestor = await factory.CriarGestorAsync();
        var request = Novo();
        var chamado = await AbrirAsync(cidadao, request);
        await MudarStatusAsync(gestor, chamado.Id, "EM_ANALISE");
        var busca = Uri.EscapeDataString(request.Titulo);

        // Act
        var emAnalise = await cidadao.GetFromJsonAsync<PaginaResponse<ChamadoResponse>>($"/api/v1/chamados?status=EM_ANALISE&busca={busca}");
        var abertos = await cidadao.GetFromJsonAsync<PaginaResponse<ChamadoResponse>>($"/api/v1/chamados?status=ABERTO&busca={busca}");

        // Assert
        Assert.Equal(1, emAnalise!.TotalElements);
        Assert.Equal(0, abertos!.TotalElements);
        Assert.Null(emAnalise.Content[0].FotoBase64); // a lista vem sem foto por padrão
    }

    [Fact]
    public async Task Excluir_PeloAutorComChamadoAberto_DeveRetornar204()
    {
        // Arrange
        var (autor, _) = await factory.CriarCidadaoAsync();
        var chamado = await AbrirAsync(autor);

        // Act
        var response = await autor.DeleteAsync($"/api/v1/chamados/{chamado.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await autor.GetAsync($"/api/v1/chamados/{chamado.Id}")).StatusCode);
    }
}