using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Tests.Unit.Domain;

public class StatusTests
{
    [Theory]
    [InlineData("PENDENTE", "PENDENTE")]
    [InlineData("Em Análise", "EM_ANALISE")]
    [InlineData(" em-execucao ", "EM_EXECUCAO")]
    public void Canonico_ComVariacoesValidas_DeveRetornarCodigoOficial(string entrada, string esperado)
    {
        // Arrange / Act
        var resultado = Status.Canonico(entrada);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData("Resolvido")]
    [InlineData("")]
    [InlineData(null)]
    public void Canonico_ComValorInvalido_DeveLancarDomainException(string? entrada)
    {
        // Arrange / Act
        var act = () => Status.Canonico(entrada);

        // Assert
        Assert.Throws<DomainException>(act);
    }
}