using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ZelaFix.Application.DTOs;
using ZelaFix.Tests.Integration.Fixtures;

namespace ZelaFix.Tests.Integration.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class AuthIntegrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_ComDadosValidos_DeveRetornar201ComTokenECargoCidadao()
    {
        // Arrange
        var request = factory.NovoCadastro();

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(auth!.Token));
        Assert.Equal("CIDADAO", auth.Usuario.Cargo);
        Assert.Equal("Sé", auth.Usuario.Bairro);
    }

    [Fact]
    public async Task Register_ComEmailDuplicado_DeveRetornar409()
    {
        // Arrange
        var request = factory.NovoCadastro();
        await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_ComCepInexistente_DeveRetornar400()
    {
        // Arrange
        var request = factory.NovoCadastro() with { Cep = "99999-999" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornar200ComToken()
    {
        // Arrange
        var cadastro = factory.NovoCadastro();
        await _client.PostAsJsonAsync("/api/v1/auth/register", cadastro);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest { Email = cadastro.Email, Senha = cadastro.Senha });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(auth!.Token));
    }

    [Fact]
    public async Task Login_ComSenhaIncorreta_DeveRetornar401()
    {
        // Arrange
        var cadastro = factory.NovoCadastro();
        await _client.PostAsJsonAsync("/api/v1/auth/register", cadastro);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest { Email = cadastro.Email, Senha = "senha-errada" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_SemToken_DeveRetornar401()
    {
        // Arrange / Act
        var response = await _client.GetAsync("/api/v1/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ComTokenInvalido_DeveRetornar401()
    {
        // Arrange
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "token.invalido.qualquer");

        // Act
        var response = await _client.SendAsync(requisicao);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ComTokenValido_DeveRetornar200ComOPerfil()
    {
        // Arrange
        var (cliente, auth) = await factory.CriarCidadaoAsync();

        // Act
        var perfil = await cliente.GetFromJsonAsync<UsuarioResponse>("/api/v1/auth/me");

        // Assert
        Assert.Equal(auth.Usuario.Id, perfil!.Id);
        Assert.NotNull(perfil.Endereco);
        Assert.Equal("01001000", perfil.Endereco!.Cep);
        Assert.Equal("Praça da Sé", perfil.Endereco.Rua);
        Assert.Equal("100", perfil.Endereco.Numero);
        Assert.Equal("Sé", perfil.Endereco.Bairro);
        Assert.Equal("São Paulo", perfil.Endereco.Cidade);
        Assert.Equal("São Paulo", perfil.Endereco.Estado);
    }
}