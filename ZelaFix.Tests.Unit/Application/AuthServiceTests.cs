using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ZelaFix.Application.DTOs;
using ZelaFix.Application.Exceptions;
using ZelaFix.Application.Repositories;
using ZelaFix.Application.Services.Implementations;
using ZelaFix.Application.Services.Interfaces;
using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Enums;

namespace ZelaFix.Tests.Unit.Application;

public class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarios = new();
    private readonly Mock<IRepository<Telefone>> _telefones = new();
    private readonly Mock<IEnderecoResolver> _enderecos = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private AuthService Criar() => new(
        _usuarios.Object, _telefones.Object, _enderecos.Object, _tokens.Object, _unitOfWork.Object,
        NullLogger<AuthService>.Instance);

    private static Usuario Maria() => new("Maria", "maria@teste.com", "senha123", Cargo.Cidadao, 1);

    [Fact]
    public async Task LoginAsync_ComSenhaIncorreta_DeveRetornarNullESemGerarToken()
    {
        // Arrange
        _usuarios.Setup(u => u.GetByEmailAsync("maria@teste.com", It.IsAny<CancellationToken>())).ReturnsAsync(Maria());

        // Act
        var resposta = await Criar().LoginAsync(new LoginRequest { Email = "maria@teste.com", Senha = "errada" });

        // Assert
        Assert.Null(resposta);
        _tokens.Verify(t => t.GerarToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ComCredenciaisValidas_DeveRetornarTokenEPerfil()
    {
        // Arrange
        _usuarios.Setup(u => u.GetByEmailAsync("maria@teste.com", It.IsAny<CancellationToken>())).ReturnsAsync(Maria());
        _tokens.Setup(t => t.GerarToken(It.IsAny<Usuario>())).Returns(new TokenEmitido("jwt-fake", DateTime.UtcNow.AddHours(1)));

        // Act
        var resposta = await Criar().LoginAsync(new LoginRequest { Email = " Maria@Teste.com ", Senha = "senha123" });

        // Assert
        Assert.NotNull(resposta);
        Assert.Equal("jwt-fake", resposta.Token);
        Assert.Equal("CIDADAO", resposta.Usuario.Cargo);
    }

    [Fact]
    public async Task RegisterAsync_ComEmailJaCadastrado_DeveLancarConflito()
    {
        // Arrange
        _usuarios.Setup(u => u.ExistsAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var request = new RegisterRequest
        {
            Nome = "Maria", Email = "maria@teste.com", Senha = "senha123",
            Telefone = "(11) 98765-4321", Cep = "01001-000", Numero = "10"
        };

        // Act
        var act = () => Criar().RegisterAsync(request);

        // Assert
        await Assert.ThrowsAsync<ConflitoException>(act);
    }
}