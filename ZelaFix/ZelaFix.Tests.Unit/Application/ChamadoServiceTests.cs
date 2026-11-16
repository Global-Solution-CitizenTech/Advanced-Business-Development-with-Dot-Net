using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ZelaFix.Application.DTOs;
using ZelaFix.Application.Exceptions;
using ZelaFix.Application.Media;
using ZelaFix.Application.Repositories;
using ZelaFix.Application.Services.Implementations;
using ZelaFix.Application.Services.Interfaces;
using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Tests.Unit.Application;

public class ChamadoServiceTests
{
    private readonly Mock<IChamadoRepository> _chamados = new();
    private readonly Mock<IRepository<Categoria>> _categorias = new();
    private readonly Mock<IRepository<Prioridade>> _prioridades = new();
    private readonly Mock<IRepository<OrgaoGoverno>> _orgaos = new();
    private readonly Mock<IRepository<HistChamado>> _historicos = new();
    private readonly Mock<IRepository<Status>> _status = new();
    private readonly Mock<IRepository<VotoChamado>> _votos = new();
    private readonly Mock<IEnderecoResolver> _enderecos = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private ChamadoService Criar()
    {
        _unitOfWork.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task<int>>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task<int>> acao, CancellationToken _) => acao());
        _unitOfWork.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task<bool>>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task<bool>> acao, CancellationToken _) => acao());

        return new ChamadoService(
            _chamados.Object, _categorias.Object, _prioridades.Object, _orgaos.Object,
            _historicos.Object, _status.Object, _votos.Object,
            _enderecos.Object, _unitOfWork.Object, NullLogger<ChamadoService>.Instance);
    }

    private static ChamadoLeitura Leitura(int id = 1, int autorId = 10, string? status = "PENDENTE") => new(
        id, "Buraco na rua", "Descrição suficiente", 1, "VIAS_E_PAVIMENTACAO",
        1, "MEDIA", 1, "SECRETARIA_OBRAS", autorId, "Maria", status, 0, 0,
        "01001000", "Praça da Sé", "10", "Sem referência", "Sé", "São Paulo", "São Paulo", DateTime.UtcNow, null);

    private void ChamadoExiste(bool existe) =>
        _chamados.Setup(c => c.ExistsAsync(It.IsAny<Expression<Func<Chamado, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existe);

    private void JaApoiou(bool apoiou) =>
        _votos.Setup(v => v.ExistsAsync(It.IsAny<Expression<Func<VotoChamado, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apoiou);

    private void LeituraDoChamado(ChamadoLeitura leitura) =>
        _chamados.Setup(c => c.ObterLeituraAsync(leitura.Id, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(leitura);

    [Fact]
    public async Task ApoiarAsync_QuandoChamadoNaoExiste_DeveRetornarNull()
    {
        // Arrange
        _chamados.Setup(c => c.ObterLeituraAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChamadoLeitura?)null);

        // Act
        var resposta = await Criar().ApoiarAsync(1, 20);

        // Assert
        Assert.Null(resposta);
    }

    [Fact]
    public async Task ApoiarAsync_QuandoUsuarioEAutor_DeveLancarAcessoNegado()
    {
        // Arrange (autorId = 10, apoiador = 10)
        LeituraDoChamado(Leitura(autorId: 10));

        // Act & Assert
        await Assert.ThrowsAsync<AcessoNegadoException>(() => Criar().ApoiarAsync(1, 10));
    }

    [Fact]
    public async Task ApoiarAsync_QuandoJaApoiou_NaoDeveAdicionarVoto()
    {
        // Arrange
        ChamadoExiste(true);
        JaApoiou(true);
        LeituraDoChamado(Leitura(autorId: 10));

        // Act
        await Criar().ApoiarAsync(1, 20);

        // Assert
        _votos.Verify(v => v.AddAsync(It.IsAny<VotoChamado>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApoiarAsync_NoPrimeiroApoio_DeveAdicionarVoto()
    {
        // Arrange
        ChamadoExiste(true);
        JaApoiou(false);
        LeituraDoChamado(Leitura(autorId: 10));

        // Act
        await Criar().ApoiarAsync(1, 20);

        // Assert
        _votos.Verify(v => v.AddAsync(It.Is<VotoChamado>(v => v.ChamadoId == 1 && v.UsuarioId == 20), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AlterarStatusAsync_ComMesmoStatusAtual_DeveLancarConflito()
    {
        // Arrange
        LeituraDoChamado(Leitura(status: "PENDENTE"));

        // Act
        var act = () => Criar().AlterarStatusAsync(1, new AlterarStatusRequest { Status = "Pendente" }, 7);

        // Assert
        await Assert.ThrowsAsync<ConflitoException>(act);
    }

    [Fact]
    public async Task AlterarStatusAsync_ComStatusInvalido_DeveLancarDomainException()
    {
        // Arrange / Act
        var act = () => Criar().AlterarStatusAsync(1, new AlterarStatusRequest { Status = "Resolvido" }, 7);

        // Assert
        await Assert.ThrowsAsync<DomainException>(act);
    }

    [Fact]
    public async Task AlterarStatusAsync_ComStatusValido_DeveRegistrarHistoricoComOGestor()
    {
        // Arrange
        LeituraDoChamado(Leitura(status: "PENDENTE"));
        var statusEmAnalise = new Status("EM_ANALISE");
        statusEmAnalise.DefinirIdParaTeste(2);
        _status.Setup(s => s.FirstOrDefaultAsync(It.IsAny<Expression<Func<Status, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusEmAnalise);

        // Act
        await Criar().AlterarStatusAsync(1, new AlterarStatusRequest { Status = "Em Análise" }, 7);

        // Assert
        _historicos.Verify(h => h.AddAsync(It.Is<HistChamado>(h => h.ChamadoId == 1 && h.UsuarioId == 7), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_PorUsuarioQueNaoEAutorNemGestor_DeveLancarAcessoNegado()
    {
        // Arrange
        LeituraDoChamado(Leitura(autorId: 10));
        var request = new AtualizarChamadoRequest
        {
            Titulo = "Novo título", Descricao = "Descrição suficiente", CategoriaId = 1, Cep = "01001-000", Numero = "10"
        };

        // Act
        var act = () => Criar().AtualizarAsync(1, request, usuarioId: 99, ehGestor: false);

        // Assert
        await Assert.ThrowsAsync<AcessoNegadoException>(act);
    }

    [Fact]
    public async Task ExcluirAsync_PeloAutorComChamadoEmAnalise_DeveLancarDomainException()
    {
        // Arrange
        LeituraDoChamado(Leitura(autorId: 10, status: "EM_ANALISE"));

        // Act
        var act = () => Criar().ExcluirAsync(1, usuarioId: 10, ehGestor: false);

        // Assert
        await Assert.ThrowsAsync<DomainException>(act);
    }

    [Fact]
    public async Task CriarAsync_ComFotoInvalida_DeveLancarDomainExceptionESemSalvarChamado()
    {
        // Arrange
        var request = new CriarChamadoRequest
        {
            Titulo = "Buraco na pista",
            Descricao = "Descrição válida da ocorrência",
            CategoriaId = 1,
            Cep = "01001-000",
            Numero = "10",
            FotoBase64 = "nao-e-base64!!!"
        };

        // Act
        var act = () => Criar().CriarAsync(request, 10);

        // Assert
        await Assert.ThrowsAsync<DomainException>(act);
        _chamados.Verify(c => c.AddAsync(It.IsAny<Chamado>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_ComDadosValidos_DeveSalvarChamadoERegistrarHistorico()
    {
        // Arrange
        _categorias.Setup(c => c.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Categoria("VIAS_E_PAVIMENTACAO"));
        var endereco = new Endereco("01001000", "Praça da Sé", "10", null, 1);
        endereco.DefinirIdParaTeste(1);
        _enderecos.Setup(e => e.ResolverAsync("01001-000", "10", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(endereco);
        var prioridade = new Prioridade("MEDIA");
        prioridade.DefinirIdParaTeste(2);
        _prioridades.Setup(p => p.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Prioridade, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(prioridade);
        var orgao = new OrgaoGoverno("SECRETARIA_OBRAS");
        orgao.DefinirIdParaTeste(1);
        _orgaos.Setup(o => o.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<OrgaoGoverno, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orgao);
        var statusPendente = new Status("PENDENTE");
        statusPendente.DefinirIdParaTeste(1);
        _status.Setup(s => s.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Status, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusPendente);

        _chamados.Setup(c => c.AddAsync(It.IsAny<Chamado>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado c, CancellationToken _) =>
            {
                c.DefinirIdParaTeste(5);
                return c;
            });

        LeituraDoChamado(Leitura(id: 5, status: "PENDENTE"));
        var request = new CriarChamadoRequest
        {
            Titulo = "Buraco profundo na pista",
            Descricao = "Descrição detalhada da ocorrência urbana.",
            CategoriaId = 1,
            Cep = "01001-000",
            Numero = "10",
            FotoBase64 = FotoImagemTests.PngBase64
        };

        // Act
        var resposta = await Criar().CriarAsync(request, 10);

        // Assert
        Assert.Equal(5, resposta.Id);
        Assert.Equal("PENDENTE", resposta.Status);
        _chamados.Verify(c => c.AddAsync(It.IsAny<Chamado>(), It.IsAny<CancellationToken>()), Times.Once);
        _historicos.Verify(h => h.AddAsync(It.IsAny<HistChamado>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterEstatisticas_DeveDelegarAoRepositorio()
    {
        // Arrange
        var esperado = new EstatisticasResponse(10, 3, 2, 4, 1, 15);
        _chamados.Setup(c => c.ObterEstatisticasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(esperado);

        // Act
        var resultado = await Criar().ObterEstatisticasAsync();

        // Assert
        Assert.Equal(esperado, resultado);
        _chamados.Verify(c => c.ObterEstatisticasAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
