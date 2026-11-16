using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Enums;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Tests.Unit.Domain;

public class UsuarioTests
{
    [Fact]
    public void Construtor_ComSenhaValida_DeveGravarHashBCryptDe60Caracteres()
    {
        // Arrange / Act
        var usuario = new Usuario("Maria", "maria@teste.com", "senha123", Cargo.Cidadao, 1);

        // Assert
        Assert.NotEqual("senha123", usuario.SenhaHash);
        Assert.Equal(60, usuario.SenhaHash.Length);
        Assert.True(usuario.VerificarSenha("senha123"));
        Assert.False(usuario.VerificarSenha("outra-senha"));
    }

    [Fact]
    public void Construtor_ComSenhaCurta_DeveLancarDomainException()
    {
        // Arrange / Act
        var act = () => new Usuario("Maria", "maria@teste.com", "123", Cargo.Cidadao, 1);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Construtor_ComEmailMaiusculo_DeveNormalizarParaMinusculo()
    {
        // Arrange / Act
        var usuario = new Usuario("Maria", "MARIA@Teste.COM", "senha123", Cargo.Cidadao, 1);

        // Assert
        Assert.Equal("maria@teste.com", usuario.Email);
    }

    [Fact]
    public void AtualizarEmail_SemArroba_DeveLancarDomainException()
    {
        // Arrange
        var usuario = new Usuario("Maria", "maria@teste.com", "senha123", Cargo.Cidadao, 1);

        // Act
        var act = () => usuario.AtualizarEmail("sem-arroba");

        // Assert
        Assert.Throws<DomainException>(act);
    }
}