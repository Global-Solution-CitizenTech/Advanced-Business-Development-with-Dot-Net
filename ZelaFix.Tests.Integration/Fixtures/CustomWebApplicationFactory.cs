using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ZelaFix.Application.DTOs;
using ZelaFix.Application.Repositories;
using ZelaFix.Application.Services.Interfaces;
using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Enums;
using ZelaFix.Infrastructure.Persistence;

namespace ZelaFix.Tests.Integration.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string ChaveJwt = "teste-integracao-zelafix-chave-jwt-com-mais-de-32-caracteres!";
    public const string EmailGestor = "gestor@zelafix.com";
    public const string SenhaGestor = "Gestor#123";

    private readonly string _banco = "ZelaFix_Teste_" + Guid.NewGuid();

    public int CategoriaId { get; private set; }

    /// <summary>Limite de tentativas de login por janela (alto por padrão; o teste de 429 usa um baixo).</summary>
    protected virtual int LimiteAuth => 1000;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:ZelaFixOracle", "User Id=x;Password=x;Data Source=localhost:1521/x;");
        builder.UseSetting("Jwt:Key", ChaveJwt);
        builder.UseSetting("RateLimiting:AuthPermitLimit", LimiteAuth.ToString());
        builder.UseSetting("RateLimiting:GlobalPermitLimit", "100000");

        builder.ConfigureServices(services =>
        {
            // Remove o DbContext do Oracle (inclusive a configuração do provider) e usa InMemory.
            foreach (var d in services.Where(d => d.ServiceType.FullName?.Contains("ZelaFixDbContext") == true).ToList())
                services.Remove(d);
            services.AddDbContext<ZelaFixDbContext>(o => o.UseInMemoryDatabase(_banco));

            services.RemoveAll<IChamadoProcedures>();
            services.AddScoped<IChamadoProcedures, FakeChamadoProcedures>();

            services.RemoveAll<IViaCepService>();
            services.AddSingleton<IViaCepService, FakeViaCepService>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        Semear(scope.ServiceProvider.GetRequiredService<ZelaFixDbContext>());
        return host;
    }

    private void Semear(ZelaFixDbContext db)
    {
        db.Status.AddRange(Status.Todos.Select(nome => new Status(nome)));

        var categoria = new Categoria("Buraco na Rua");
        db.Categorias.Add(categoria);

        var estado = new Estado("São Paulo");
        db.Estados.Add(estado);
        db.SaveChanges();

        var cidade = new Cidade("São Paulo", estado.Id);
        db.Cidades.Add(cidade);
        db.SaveChanges();

        var bairro = new Bairro("Sé", cidade.Id);
        db.Bairros.Add(bairro);
        db.SaveChanges();

        var endereco = new Endereco("01001000", "Praça da Sé", "1", null, bairro.Id);
        db.Enderecos.Add(endereco);
        db.SaveChanges();

        db.Usuarios.Add(new Usuario("Gestor Teste", EmailGestor, SenhaGestor, Cargo.Gestor, endereco.Id));
        db.SaveChanges();

        CategoriaId = categoria.Id;
    }

    public RegisterRequest NovoCadastro() => new()
    {
        Nome = "Maria Silva",
        Email = $"{Guid.NewGuid():N}@teste.com",
        Senha = "senha123",
        Telefone = "(11) 98765-4321",
        Cep = "01001-000",
        Numero = "100"
    };

    /// <summary>Cadastra um cidadão novo pela API e devolve um cliente já autenticado.</summary>
    public async Task<(HttpClient Cliente, AuthResponse Auth)> CriarCidadaoAsync()
    {
        var cliente = CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/api/v1/auth/register", NovoCadastro());
        resposta.EnsureSuccessStatusCode();

        var auth = (await resposta.Content.ReadFromJsonAsync<AuthResponse>())!;
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return (cliente, auth);
    }

    public async Task<HttpClient> CriarGestorAsync()
    {
        var cliente = CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest { Email = EmailGestor, Senha = SenhaGestor });
        resposta.EnsureSuccessStatusCode();

        var auth = (await resposta.Content.ReadFromJsonAsync<AuthResponse>())!;
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return cliente;
    }
}

/// <summary>Variante com limite de login baixo, para provar o HTTP 429.</summary>
public sealed class RateLimitedWebApplicationFactory : CustomWebApplicationFactory
{
    protected override int LimiteAuth => 2;
}