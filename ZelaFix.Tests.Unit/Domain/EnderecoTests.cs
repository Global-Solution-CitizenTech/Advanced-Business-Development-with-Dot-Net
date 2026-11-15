using ZelaFix.Domain.Common;
using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Tests.Unit.Domain;

public class EnderecoChamadoPrioridadeTests
{
    [Fact]
    public void Endereco_ComCepMascarado_DeveGuardarSomenteDigitos()
    {
        // Arrange / Act
        var endereco = new Endereco("01001-000", "Praça da Sé", "10", "Perto do metrô", 1);

        // Assert
        Assert.Equal("01001000", endereco.Cep);
    }

    [Fact]
    public void Endereco_ComCepInvalido_DeveLancarDomainException()
    {
        // Arrange / Act
        var act = () => new Endereco("123", "Rua", "10", null, 1);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Endereco_SemReferencia_DeveUsarTextoPadraoPoisOracleTrataVazioComoNulo()
    {
        // Arrange / Act
        var endereco = new Endereco("01001000", "Rua", "10", "   ", 1);

        // Assert
        Assert.Equal(Endereco.SemReferencia, endereco.Referencia);
    }

    [Fact]
    public void Chamado_SemFoto_DeveLancarDomainException()
    {
        // Arrange / Act
        var act = () => new Chamado("Buraco na rua", "Descrição suficiente", [], 1, 1, 1, 1, 1);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Chamado_ComDadosValidos_DeveRegistrarAberturaEmUtc()
    {
        // Arrange
        var antes = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var chamado = new Chamado("Buraco na rua", "Descrição suficiente", [1, 2, 3], 1, 1, 1, 1, 1);

        // Assert
        Assert.True(chamado.DataAbertura >= antes);
        Assert.Equal("Buraco na rua", chamado.Titulo);
    }

    [Theory]
    [InlineData(0, "Baixa")]
    [InlineData(4, "Baixa")]
    [InlineData(5, "Média")]
    [InlineData(15, "Alta")]
    [InlineData(25, "Crítica")]
    public void Calcular_PorQuantidadeDeApoios_DeveRetornarPrioridadeDaFaixa(int apoios, string esperada)
    {
        // Arrange / Act
        var prioridade = PoliticaPrioridade.Calcular(apoios);

        // Assert
        Assert.Equal(esperada, prioridade);
    }
}