using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using ZelaFix.Api.Exceptions;
using ZelaFix.Api.Idempotency;
using ZelaFix.Application.DTOs;
using ZelaFix.Application.Exceptions;
using ZelaFix.Application.Repositories;
using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Exceptions;
using ZelaFix.Infrastructure.Persistence;
using ZelaFix.Tests.Integration.Fixtures;

namespace ZelaFix.Tests.Integration.Endpoints;

public class RepositoriesAndInfrastructureTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private sealed class FakeHostEnv(string envName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = envName;
        public string ApplicationName { get; set; } = "ZelaFix.Api";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    [Fact]
    public async Task RepositoryAndChamadoRepository_ConsultasEFiltros_DevemExecutarComSucesso()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var catRepo = scope.ServiceProvider.GetRequiredService<IRepository<Categoria>>();
        var chamadoRepo = scope.ServiceProvider.GetRequiredService<IChamadoRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Act & Assert - Repository<T>
        var todas = await catRepo.GetAllAsync();
        Assert.NotEmpty(todas);

        var encontradas = await catRepo.FindAsync(c => c.Nome == "VIAS_E_PAVIMENTACAO");
        Assert.Single(encontradas);

        var contagem = await catRepo.CountAsync(c => c.Id > 0);
        Assert.True(contagem >= 1);

        var deletouInexistente = await catRepo.DeleteAsync(999999);
        Assert.False(deletouInexistente);

        // Act & Assert - UnitOfWork + ChamadoRepository Ordenar MaisApoiados & BairroId
        var exec = await uow.ExecuteInTransactionAsync(() => Task.FromResult(42));
        Assert.Equal(42, exec);

        var pagina = await chamadoRepo.BuscarAsync(new ChamadoFiltro
        {
            BairroId = 1,
            Ordenar = ChamadoOrdenacao.MaisApoiados,
            Page = 0,
            Size = 5
        }, 1);
        Assert.NotNull(pagina);

        var fotoNula = await chamadoRepo.GetFotoAsync(999999);
        Assert.Null(fotoNula);

        // DbInitializer com InMemory (não relacional) deve retornar sem erro
        await DbInitializer.InitializeAsync(factory.Services, NullLogger.Instance);

        // Semeadura idempotente das contas padrão (CIDADAO e GESTOR)
        var db = scope.ServiceProvider.GetRequiredService<ZelaFixDbContext>();
        await DbInitializer.SemearUsuariosPadraoAsync(db);
        await DbInitializer.SemearUsuariosPadraoAsync(db);
        Assert.NotNull(await db.Usuarios.FirstOrDefaultAsync(u => u.Email == "gestor@zelafix.com.br"));
        Assert.NotNull(await db.Usuarios.FirstOrDefaultAsync(u => u.Email == "cidadao@zelafix.com.br"));
    }

    [Theory]
    [InlineData(typeof(DomainException), StatusCodes.Status400BadRequest, "Production")]
    [InlineData(typeof(ConflitoException), StatusCodes.Status409Conflict, "Production")]
    [InlineData(typeof(AcessoNegadoException), StatusCodes.Status403Forbidden, "Production")]
    [InlineData(typeof(ArgumentException), StatusCodes.Status400BadRequest, "Production")]
    [InlineData(typeof(KeyNotFoundException), StatusCodes.Status404NotFound, "Production")]
    [InlineData(typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized, "Production")]
    [InlineData(typeof(DbUpdateException), StatusCodes.Status409Conflict, "Production")]
    [InlineData(typeof(DbUpdateException), StatusCodes.Status409Conflict, "Development")]
    [InlineData(typeof(HttpRequestException), StatusCodes.Status502BadGateway, "Production")]
    [InlineData(typeof(TimeoutException), StatusCodes.Status504GatewayTimeout, "Production")]
    [InlineData(typeof(InvalidOperationException), StatusCodes.Status500InternalServerError, "Production")]
    [InlineData(typeof(InvalidOperationException), StatusCodes.Status500InternalServerError, "Development")]
    public async Task GlobalExceptionHandler_DiversasExcecoes_DeveMapearStatusCorreto(
        Type exceptionType, int expectedStatus, string envName)
    {
        // Arrange
        var handler = new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance,
            new FakeHostEnv(envName));

        var http = new DefaultHttpContext();
        http.Response.Body = new MemoryStream();
        http.Request.Method = "GET";
        http.Request.Path = "/teste";

        var ex = (Exception)Activator.CreateInstance(exceptionType, "Mensagem teste")!;

        // Act
        var handled = await handler.TryHandleAsync(http, ex, CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(expectedStatus, http.Response.StatusCode);
    }

    [Fact]
    public async Task GlobalExceptionHandler_TaskCanceledComTimeout_DeveRetornar504()
    {
        var handler = new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance,
            new FakeHostEnv("Production"));

        var http = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        var ex = new TaskCanceledException("Timeout", new TimeoutException("inner"));

        var handled = await handler.TryHandleAsync(http, ex, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status504GatewayTimeout, http.Response.StatusCode);
    }

    [Fact]
    public async Task MemoryIdempotencyStore_CicloDeVidaCompleto_DeveGerenciarEstados()
    {
        // Arrange
        var store = new MemoryIdempotencyStore();
        var chave = Guid.NewGuid().ToString();

        // Act & Assert: Novo
        var inicio = await store.IniciarAsync(chave, "hash-a");
        Assert.Equal(EstadoIdempotencia.Novo, inicio.Estado);

        // Payload diferente com chave em andamento
        var diff = await store.IniciarAsync(chave, "hash-b");
        Assert.Equal(EstadoIdempotencia.PayloadDiferente, diff.Estado);

        // Mesmo payload enquanto em andamento
        var emAndamento = await store.IniciarAsync(chave, "hash-a");
        Assert.Equal(EstadoIdempotencia.EmAndamento, emAndamento.Estado);

        // Concluir e consultar replay
        var stored = new RespostaArmazenada(201, "{\"id\":1}");
        await store.ConcluirAsync(chave, stored);

        var concluido = await store.IniciarAsync(chave, "hash-a");
        Assert.Equal(EstadoIdempotencia.Concluido, concluido.Estado);
        Assert.Equal(201, concluido.Resposta!.StatusCode);

        // Liberar chave e reinserir
        await store.LiberarAsync(chave);
        var reinserido = await store.IniciarAsync(chave, "hash-a");
        Assert.Equal(EstadoIdempotencia.Novo, reinserido.Estado);
    }
}
