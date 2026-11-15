using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ZelaFix.Application.DTOs;
using ZelaFix.Application.Exceptions;
using ZelaFix.Application.Repositories;
using ZelaFix.Application.Services.Implementations;
using ZelaFix.Domain.Entities;

namespace ZelaFix.Tests.Unit.Application;

public class CategoriaServiceTests
{
    private readonly Mock<IRepository<Categoria>> _repo = new();

    private CategoriaService Criar() => new(_repo.Object, NullLogger<CategoriaService>.Instance);

    private void ExisteNome(bool existe) =>
        _repo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Categoria, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existe);

    [Fact]
    public async Task CreateAsync_ComNomeDuplicado_DeveLancarConflitoENaoPersistir()
    {
        // Arrange
        ExisteNome(true);

        // Act
        var act = () => Criar().CreateAsync(new CategoriaRequest { Nome = "ILUMINACAO_PUBLICA" });

        // Assert
        await Assert.ThrowsAsync<ConflitoException>(act);
        _repo.Verify(r => r.AddAsync(It.IsAny<Categoria>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ComNomeNovo_DevePersistirERetornarResposta()
    {
        // Arrange
        ExisteNome(false);
        _repo.Setup(r => r.AddAsync(It.IsAny<Categoria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Categoria c, CancellationToken _) => c);

        // Act
        var resposta = await Criar().CreateAsync(new CategoriaRequest { Nome = "  iluminacao_publica " });

        // Assert
        Assert.Equal("ILUMINACAO_PUBLICA", resposta.Nome);
        _repo.Verify(r => r.AddAsync(It.IsAny<Categoria>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ComCategoriaInexistente_DeveRetornarNull()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Categoria?)null);

        // Act
        var resposta = await Criar().UpdateAsync(99, new CategoriaRequest { Nome = "Outra" });

        // Assert
        Assert.Null(resposta);
    }
}