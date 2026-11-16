using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using ZelaFix.Application.Auth;
using ZelaFix.Application.DTOs;
using ZelaFix.Application.Media;
using ZelaFix.Application.Services.Implementations;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Tests.Unit.Application;

public class ViaCepAndRegisterTests
{
    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(handler(request));
    }

    [Fact]
    public async Task ViaCepService_ComCepValido_DeveRetornarDados()
    {
        // Arrange
        var json = """{"cep":"01001-000","logradouro":"Praça da Sé","bairro":"Sé","localidade":"São Paulo","uf":"SP"}""";
        var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            }))
        { BaseAddress = new Uri("https://viacep.com.br/") };

        var service = new ViaCepService(client, NullLogger<ViaCepService>.Instance);

        // Act
        var resp = await service.ConsultarAsync("01001000");

        // Assert
        Assert.NotNull(resp);
        Assert.Equal("Sé", resp!.Bairro);
        Assert.Equal("São Paulo", resp.Localidade);
        Assert.False(resp.PossuiErro);
    }

    [Fact]
    public async Task ViaCepService_ComErroOuBadRequest_DeveRetornarNull()
    {
        // Arrange - erro: true
        var clientErro = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"erro": true}""", Encoding.UTF8, "application/json")
            }))
        { BaseAddress = new Uri("https://viacep.com.br/") };

        var svcErro = new ViaCepService(clientErro, NullLogger<ViaCepService>.Instance);

        // Arrange - 400 BadRequest
        var client400 = new HttpClient(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)))
        { BaseAddress = new Uri("https://viacep.com.br/") };
        var svc400 = new ViaCepService(client400, NullLogger<ViaCepService>.Instance);

        // Act & Assert
        Assert.Null(await svcErro.ConsultarAsync("99999999"));
        Assert.Null(await svc400.ConsultarAsync("000"));
    }

    [Fact]
    public void RegisterRequest_CompatibilidadeMobileEValidacao_DeveResolverEValidarCampos()
    {
        // Arrange
        var mobileReq = new RegisterRequest
        {
            Nome = "Maria Mobile",
            Email = "maria@mobile.com",
            Senha = "Password123",
            Ddd = "11",
            NumeroTelefone = "988887777",
            Endereco = new RegisterEnderecoInput("01001000", "42", "Esquina", "Praça da Sé")
        };

        // Act & Assert
        Assert.Equal("(11) 988887777", mobileReq.ObterTelefoneResolvido());
        Assert.Equal("01001000", mobileReq.ObterCepResolvido());
        Assert.Equal("42", mobileReq.ObterNumeroResolvido());
        Assert.Equal("Esquina", mobileReq.ObterReferenciaResolvida());
        Assert.Equal(("11", "988887777"), mobileReq.SepararTelefone());
        Assert.Empty(mobileReq.Validate(new ValidationContext(mobileReq)));

        var invalido = new RegisterRequest { Nome = "Teste", Email = "t@t.com", Senha = "Password123", Numero = "" };
        var erros = invalido.Validate(new ValidationContext(invalido)).ToList();
        Assert.Contains(erros, e => e.MemberNames.Contains(nameof(RegisterRequest.Telefone)));
        Assert.Contains(erros, e => e.MemberNames.Contains(nameof(RegisterRequest.Cep)));
    }

    [Fact]
    public void FotoImagem_CasosBorda_DeveDetectarFormatosERejeitarInvalidos()
    {
        // Data URI sem vírgula / Base64 vazio / >5MB / formato inválido
        Assert.Throws<DomainException>(() => FotoImagem.Decodificar("data:image/jpeg;base64"));
        Assert.Throws<DomainException>(() => FotoImagem.Decodificar("not-valid-base64!!!"));
        Assert.Throws<DomainException>(() => FotoImagem.Decodificar(Convert.ToBase64String(Array.Empty<byte>())));

        var oversized = new byte[FotoImagem.TamanhoMaximoBytes + 1];
        Assert.Throws<DomainException>(() => FotoImagem.Decodificar(Convert.ToBase64String(oversized)));

        // WebP signature (RIFF....WEBP)
        var webp = new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F', 0, 0, 0, 0, (byte)'W', (byte)'E', (byte)'B', (byte)'P', 0 };
        var decWebp = FotoImagem.Decodificar(Convert.ToBase64String(webp));
        Assert.Equal("image/webp", decWebp.ContentType);

        // JPEG signature
        var jpg = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        Assert.Equal("image/jpeg", FotoImagem.DetectarContentType(jpg));
    }

    [Fact]
    public void JwtSettings_Validacao_DeveExigirChaveMinimaEExpiracaoPositiva()
    {
        var invalidKey = new JwtSettings { Key = "curta", ExpirationMinutes = 10 };
        Assert.Throws<InvalidOperationException>(() => invalidKey.Validar());

        var invalidExp = new JwtSettings { Key = new string('x', 32), ExpirationMinutes = 0 };
        Assert.Throws<InvalidOperationException>(() => invalidExp.Validar());

        var valid = new JwtSettings { Key = new string('x', 32), ExpirationMinutes = 60 };
        valid.Validar();
    }
}
