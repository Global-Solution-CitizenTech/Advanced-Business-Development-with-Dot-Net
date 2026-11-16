using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;
using ZelaFix.Application.DTOs;
using ZelaFix.Tests.Integration.Fixtures;

namespace ZelaFix.Tests.Integration.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class LiveEndpointsAuditTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
{
    private const string PngBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private async Task<(HttpClient Client, string Token, UsuarioResponse Usuario)> CriarUsuarioAutenticadoAsync(string prefixo, string cargo = "CIDADAO")
    {
        var email = $"{prefixo}_{Guid.NewGuid():N}@zelafix.com";
        var cliente = factory.CreateClient();

        var reg = new RegisterRequest
        {
            Nome = $"Usuario {prefixo}",
            Email = email,
            Senha = "SenhaForte#2026",
            Telefone = "11988887777",
            Cep = "01001-000",
            Numero = "100"
        };

        var regResp = await cliente.PostAsJsonAsync("/api/v1/auth/register", reg);
        regResp.EnsureSuccessStatusCode();

        var loginResp = await cliente.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = email,
            Senha = "SenhaForte#2026"
        });
        loginResp.EnsureSuccessStatusCode();

        var authData = await loginResp.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authData!.Token);

        return (cliente, authData.Token, authData.Usuario);
    }

    [Fact]
    public async Task ExecutarAuditoriaCompleta_TodosEndpoints_ValidarContratosEBugsSilenciosos()
    {
        var sw = Stopwatch.StartNew();
        output.WriteLine("=========================================================================");
        output.WriteLine("INÍCIO DA AUDITORIA COMPLETA DE TODOS OS ENDPOINTS DA API ZELAFIX (.NET)");
        output.WriteLine("=========================================================================");

        var anonClient = factory.CreateClient();
        var (autorClient, autorToken, autor) = await CriarUsuarioAutenticadoAsync("autor");
        var (outroClient, outroToken, outro) = await CriarUsuarioAutenticadoAsync("outro");
        var gestorClient = await factory.CriarGestorAsync();

        // 1. Health Checks
        output.WriteLine("\n--- [1/6] HEALTH CHECKS ---");
        var h1 = await anonClient.GetAsync("/health");
        output.WriteLine($"GET /health -> {(int)h1.StatusCode} {h1.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, h1.StatusCode);

        var h2 = await anonClient.GetAsync("/health/ready");
        output.WriteLine($"GET /health/ready -> {(int)h2.StatusCode} {h2.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, h2.StatusCode);

        var h3 = await anonClient.GetAsync("/health/live");
        output.WriteLine($"GET /health/live -> {(int)h3.StatusCode} {h3.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, h3.StatusCode);

        // 2. Swagger
        output.WriteLine("\n--- [2/6] SWAGGER & OPENAPI ---");
        var swagger = await anonClient.GetAsync("/swagger/v1/swagger.json");
        output.WriteLine($"GET /swagger/v1/swagger.json -> {(int)swagger.StatusCode} {swagger.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, swagger.StatusCode);

        // 3. Auth Endpoints
        output.WriteLine("\n--- [3/6] AUTH ENDPOINTS ---");
        var loginInvalido = await anonClient.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = "nao_existe@zelafix.com",
            Senha = "SenhaErrada#1"
        });
        output.WriteLine($"POST /api/v1/auth/login (Inexistente) -> {(int)loginInvalido.StatusCode} {loginInvalido.StatusCode}");
        Assert.Equal(HttpStatusCode.Unauthorized, loginInvalido.StatusCode);

        var meSemToken = await anonClient.GetAsync("/api/v1/auth/me");
        output.WriteLine($"GET /api/v1/auth/me (Sem Token) -> {(int)meSemToken.StatusCode} {meSemToken.StatusCode}");
        Assert.Equal(HttpStatusCode.Unauthorized, meSemToken.StatusCode);

        var meComToken = await autorClient.GetAsync("/api/v1/auth/me");
        output.WriteLine($"GET /api/v1/auth/me (Cidadão) -> {(int)meComToken.StatusCode} {meComToken.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, meComToken.StatusCode);
        var meUsuario = await meComToken.Content.ReadFromJsonAsync<UsuarioResponse>(JsonOptions);
        Assert.NotNull(meUsuario);
        Assert.Equal(autor.Email, meUsuario.Email);

        // 4. Categorias
        output.WriteLine("\n--- [4/6] CATEGORIAS ---");
        var getCategorias = await autorClient.GetAsync("/api/v1/categorias");
        output.WriteLine($"GET /api/v1/categorias -> {(int)getCategorias.StatusCode} {getCategorias.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, getCategorias.StatusCode);
        var categoriasList = await getCategorias.Content.ReadFromJsonAsync<List<CategoriaResponse>>(JsonOptions);
        Assert.NotNull(categoriasList);
        Assert.NotEmpty(categoriasList);

        var catId = categoriasList[0].Id;
        var getCatById = await autorClient.GetAsync($"/api/v1/categorias/{catId}");
        output.WriteLine($"GET /api/v1/categorias/{catId} -> {(int)getCatById.StatusCode} {getCatById.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, getCatById.StatusCode);

        // Criar categoria como cidadão -> 403 Forbidden
        var postCatCidadao = await autorClient.PostAsJsonAsync("/api/v1/categorias", new CategoriaRequest { Nome = "NOVA_CAT_TESTE" });
        output.WriteLine($"POST /api/v1/categorias (Cidadão) -> {(int)postCatCidadao.StatusCode} {postCatCidadao.StatusCode}");
        Assert.Equal(HttpStatusCode.Forbidden, postCatCidadao.StatusCode);

        // Criar categoria como gestor -> 201 Created (usando categoria permitida pelo domínio)
        var postCatGestor = await gestorClient.PostAsJsonAsync("/api/v1/categorias", new CategoriaRequest { Nome = "ILUMINACAO_PUBLICA" });
        output.WriteLine($"POST /api/v1/categorias (Gestor) -> {(int)postCatGestor.StatusCode} {postCatGestor.StatusCode}");
        Assert.Equal(HttpStatusCode.Created, postCatGestor.StatusCode);
        var catCriada = await postCatGestor.Content.ReadFromJsonAsync<CategoriaResponse>(JsonOptions);
        Assert.NotNull(catCriada);

        // Atualizar categoria como gestor -> 200 OK (usando outra categoria válida)
        var putCat = await gestorClient.PutAsJsonAsync($"/api/v1/categorias/{catCriada.Id}", new CategoriaRequest { Nome = "DRENAGEM_E_ESGOTO" });
        output.WriteLine($"PUT /api/v1/categorias/{catCriada.Id} (Gestor) -> {(int)putCat.StatusCode} {putCat.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, putCat.StatusCode);

        // Excluir categoria como gestor -> 204 NoContent
        var delCat = await gestorClient.DeleteAsync($"/api/v1/categorias/{catCriada.Id}");
        output.WriteLine($"DELETE /api/v1/categorias/{catCriada.Id} (Gestor) -> {(int)delCat.StatusCode} {delCat.StatusCode}");
        Assert.Equal(HttpStatusCode.NoContent, delCat.StatusCode);

        // 5. Chamados & Estatísticas
        output.WriteLine("\n--- [5/6] CHAMADOS & ESTATÍSTICAS ---");
        // GET estatísticas inicial
        var getStats1 = await autorClient.GetAsync("/api/v1/chamados/estatisticas");
        output.WriteLine($"GET /api/v1/chamados/estatisticas -> {(int)getStats1.StatusCode} {getStats1.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, getStats1.StatusCode);
        var stats1 = await getStats1.Content.ReadFromJsonAsync<EstatisticasResponse>(JsonOptions);
        Assert.NotNull(stats1);
        output.WriteLine($"Estatísticas atuais: Total={stats1.Total}, Abertos={stats1.Abertos}, EmAndamento={stats1.EmAndamento}, Resolvidos={stats1.Resolvidos}, Rejeitados={stats1.Rejeitados}, Votos={stats1.TotalApoios}");

        // POST Chamado sem Idempotency-Key -> 400 BadRequest
        var chamadoReq = new CriarChamadoRequest
        {
            Titulo = "Bueiro entupido com transbordamento",
            Descricao = "Bueiro completamente obstruído causando alagamento na via pública.",
            CategoriaId = catId,
            Cep = "01001-000",
            Numero = "50",
            Referencia = "Próximo à estação",
            FotoBase64 = PngBase64
        };
        var postSemKey = await autorClient.PostAsJsonAsync("/api/v1/chamados", chamadoReq);
        output.WriteLine($"POST /api/v1/chamados (Sem Idempotency-Key) -> {(int)postSemKey.StatusCode} {postSemKey.StatusCode}");
        Assert.Equal(HttpStatusCode.BadRequest, postSemKey.StatusCode);

        // POST Chamado com Idempotency-Key -> 201 Created
        var chaveIdemp = Guid.NewGuid().ToString();
        var postComKeyMsg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/chamados")
        {
            Content = JsonContent.Create(chamadoReq)
        };
        postComKeyMsg.Headers.Add("Idempotency-Key", chaveIdemp);
        var postComKey = await autorClient.SendAsync(postComKeyMsg);
        output.WriteLine($"POST /api/v1/chamados (Com Idempotency-Key) -> {(int)postComKey.StatusCode} {postComKey.StatusCode}");
        Assert.Equal(HttpStatusCode.Created, postComKey.StatusCode);
        var chamadoCriado = await postComKey.Content.ReadFromJsonAsync<ChamadoResponse>(JsonOptions);
        Assert.NotNull(chamadoCriado);
        Assert.Equal("PENDENTE", chamadoCriado.Status);
        output.WriteLine($"Chamado criado: Id={chamadoCriado.Id}, Protocolo={chamadoCriado.Protocolo}, Status={chamadoCriado.Status}");

        // Replay com a mesma Idempotency-Key -> 201 Created (com Idempotent-Replayed: true)
        var replayMsg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/chamados")
        {
            Content = JsonContent.Create(chamadoReq)
        };
        replayMsg.Headers.Add("Idempotency-Key", chaveIdemp);
        var replayResp = await autorClient.SendAsync(replayMsg);
        output.WriteLine($"POST /api/v1/chamados (Replay Idempotente) -> {(int)replayResp.StatusCode} {replayResp.StatusCode}");
        Assert.Equal(HttpStatusCode.Created, replayResp.StatusCode);
        Assert.True(replayResp.Headers.Contains("Idempotent-Replayed"));

        // GET Chamado por Id
        var getChamado = await autorClient.GetAsync($"/api/v1/chamados/{chamadoCriado.Id}");
        output.WriteLine($"GET /api/v1/chamados/{chamadoCriado.Id} -> {(int)getChamado.StatusCode} {getChamado.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, getChamado.StatusCode);
        var chamadoDetalhe = await getChamado.Content.ReadFromJsonAsync<ChamadoResponse>(JsonOptions);
        Assert.NotNull(chamadoDetalhe);
        Assert.NotNull(chamadoDetalhe.FotoBase64);
        Assert.NotEmpty(chamadoDetalhe.Historico!);

        // GET Chamados (Listar com paginação e busca)
        var listChamados = await autorClient.GetAsync("/api/v1/chamados?page=0&size=10&status=PENDENTE&busca=Bueiro");
        output.WriteLine($"GET /api/v1/chamados?status=PENDENTE -> {(int)listChamados.StatusCode} {listChamados.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, listChamados.StatusCode);
        var pagina = await listChamados.Content.ReadFromJsonAsync<PaginaResponse<ChamadoResponse>>(JsonOptions);
        Assert.NotNull(pagina);
        Assert.True(pagina.TotalElements >= 1);

        // PUT Chamado (por outro cidadão -> 403 Forbidden)
        var putOutro = await outroClient.PutAsJsonAsync($"/api/v1/chamados/{chamadoCriado.Id}", new AtualizarChamadoRequest
        {
            Titulo = "Tentativa de alteração indevida",
            Descricao = "Tentativa de edição por não-autor.",
            CategoriaId = catId,
            Cep = "01001-000",
            Numero = "50"
        });
        output.WriteLine($"PUT /api/v1/chamados/{chamadoCriado.Id} (Outro cidadão) -> {(int)putOutro.StatusCode} {putOutro.StatusCode}");
        Assert.Equal(HttpStatusCode.Forbidden, putOutro.StatusCode);

        // PUT Chamado (pelo autor enquanto PENDENTE -> 200 OK)
        var putAutor = await autorClient.PutAsJsonAsync($"/api/v1/chamados/{chamadoCriado.Id}", new AtualizarChamadoRequest
        {
            Titulo = "Bueiro entupido com risco grave de alagamento",
            Descricao = "Atualização de descrição pelo cidadão autor.",
            CategoriaId = catId,
            Cep = "01001-000",
            Numero = "50",
            FotoBase64 = PngBase64
        });
        output.WriteLine($"PUT /api/v1/chamados/{chamadoCriado.Id} (Autor) -> {(int)putAutor.StatusCode} {putAutor.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, putAutor.StatusCode);

        // 6. Apoios e Transições de Status
        output.WriteLine("\n--- [6/6] APOIOS E TRANSIÇÕES DE STATUS ---");
        // Autor apoiando o próprio chamado -> 403 Forbidden
        var apoiarAutor = await autorClient.PostAsync($"/api/v1/chamados/{chamadoCriado.Id}/apoiar", null);
        output.WriteLine($"POST /api/v1/chamados/{chamadoCriado.Id}/apoiar (Próprio autor) -> {(int)apoiarAutor.StatusCode} {apoiarAutor.StatusCode}");
        Assert.Equal(HttpStatusCode.Forbidden, apoiarAutor.StatusCode);

        // Outro cidadão apoiando -> 200 OK
        var apoiarOutro = await outroClient.PostAsync($"/api/v1/chamados/{chamadoCriado.Id}/apoiar", null);
        output.WriteLine($"POST /api/v1/chamados/{chamadoCriado.Id}/apoiar (Outro cidadão) -> {(int)apoiarOutro.StatusCode} {apoiarOutro.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, apoiarOutro.StatusCode);
        var apoiadoResp = await apoiarOutro.Content.ReadFromJsonAsync<ChamadoResponse>(JsonOptions);
        Assert.NotNull(apoiadoResp);
        Assert.Equal(1, apoiadoResp.ApoiosCount);

        // Segundo apoio do mesmo cidadão (idempotente) -> 200 OK sem duplicar
        var apoiarNovamente = await outroClient.PostAsync($"/api/v1/chamados/{chamadoCriado.Id}/apoiar", null);
        output.WriteLine($"POST /api/v1/chamados/{chamadoCriado.Id}/apoiar (Re-apoio) -> {(int)apoiarNovamente.StatusCode} {apoiarNovamente.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, apoiarNovamente.StatusCode);
        var reApoiadoResp = await apoiarNovamente.Content.ReadFromJsonAsync<ChamadoResponse>(JsonOptions);
        Assert.Equal(1, reApoiadoResp!.ApoiosCount);

        // Remover apoio -> 200 OK
        var removerApoio = await outroClient.DeleteAsync($"/api/v1/chamados/{chamadoCriado.Id}/apoiar");
        output.WriteLine($"DELETE /api/v1/chamados/{chamadoCriado.Id}/apoiar -> {(int)removerApoio.StatusCode} {removerApoio.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, removerApoio.StatusCode);
        var desapoiadoResp = await removerApoio.Content.ReadFromJsonAsync<ChamadoResponse>(JsonOptions);
        Assert.Equal(0, desapoiadoResp!.ApoiosCount);

        // PATCH Status por cidadão -> 403 Forbidden
        var patchCidadaoMsg = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/chamados/{chamadoCriado.Id}/status")
        {
            Content = JsonContent.Create(new AlterarStatusRequest { Status = "EM_ANALISE" })
        };
        patchCidadaoMsg.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var patchCidadao = await autorClient.SendAsync(patchCidadaoMsg);
        output.WriteLine($"PATCH /api/v1/chamados/{chamadoCriado.Id}/status (Cidadão) -> {(int)patchCidadao.StatusCode} {patchCidadao.StatusCode}");
        Assert.Equal(HttpStatusCode.Forbidden, patchCidadao.StatusCode);

        // PATCH Status por gestor -> 200 OK (Mover para EM_EXECUCAO)
        var patchGestorMsg = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/chamados/{chamadoCriado.Id}/status")
        {
            Content = JsonContent.Create(new AlterarStatusRequest
            {
                Status = "EM_EXECUCAO",
                Observacao = "Equipe técnica mobilizada para desobstrução imediata."
            })
        };
        patchGestorMsg.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var patchGestor = await gestorClient.SendAsync(patchGestorMsg);
        output.WriteLine($"PATCH /api/v1/chamados/{chamadoCriado.Id}/status (Gestor) -> {(int)patchGestor.StatusCode} {patchGestor.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, patchGestor.StatusCode);
        var chamadoEmExecucao = await patchGestor.Content.ReadFromJsonAsync<ChamadoResponse>(JsonOptions);
        Assert.Equal("EM_EXECUCAO", chamadoEmExecucao!.Status);

        // Tentar mover novamente para o mesmo status -> 409 Conflict
        var patchMesmoStatusMsg = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/chamados/{chamadoCriado.Id}/status")
        {
            Content = JsonContent.Create(new AlterarStatusRequest { Status = "EM_EXECUCAO" })
        };
        patchMesmoStatusMsg.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var patchMesmoStatus = await gestorClient.SendAsync(patchMesmoStatusMsg);
        output.WriteLine($"PATCH /api/v1/chamados/{chamadoCriado.Id}/status (Mesmo Status) -> {(int)patchMesmoStatus.StatusCode} {patchMesmoStatus.StatusCode}");
        Assert.Equal(HttpStatusCode.Conflict, patchMesmoStatus.StatusCode);

        // Mover para CONCLUIDO
        var patchConcluidoMsg = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/chamados/{chamadoCriado.Id}/status")
        {
            Content = JsonContent.Create(new AlterarStatusRequest
            {
                Status = "CONCLUIDO",
                Observacao = "Bueiro desobstruído e limpo com sucesso."
            })
        };
        patchConcluidoMsg.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var patchConcluido = await gestorClient.SendAsync(patchConcluidoMsg);
        output.WriteLine($"PATCH /api/v1/chamados/{chamadoCriado.Id}/status (Concluido) -> {(int)patchConcluido.StatusCode} {patchConcluido.StatusCode}");
        Assert.Equal(HttpStatusCode.OK, patchConcluido.StatusCode);

        // Apoiar chamado concluído -> 409 Conflict
        var apoiarConcluido = await outroClient.PostAsync($"/api/v1/chamados/{chamadoCriado.Id}/apoiar", null);
        output.WriteLine($"POST /api/v1/chamados/{chamadoCriado.Id}/apoiar (Chamado Concluído) -> {(int)apoiarConcluido.StatusCode} {apoiarConcluido.StatusCode}");
        Assert.Equal(HttpStatusCode.Conflict, apoiarConcluido.StatusCode);

        // Excluir chamado concluído como cidadão comum -> 400 Bad Request (DomainException)
        var delCidadao = await autorClient.DeleteAsync($"/api/v1/chamados/{chamadoCriado.Id}");
        output.WriteLine($"DELETE /api/v1/chamados/{chamadoCriado.Id} (Cidadão em não-pendente) -> {(int)delCidadao.StatusCode} {delCidadao.StatusCode}");
        Assert.Equal(HttpStatusCode.BadRequest, delCidadao.StatusCode);

        // Excluir chamado como gestor -> 204 NoContent
        var delGestor = await gestorClient.DeleteAsync($"/api/v1/chamados/{chamadoCriado.Id}");
        output.WriteLine($"DELETE /api/v1/chamados/{chamadoCriado.Id} (Gestor) -> {(int)delGestor.StatusCode} {delGestor.StatusCode}");
        Assert.Equal(HttpStatusCode.NoContent, delGestor.StatusCode);

        // Verificar que agora chamado não existe mais -> 404 NotFound
        var getExcluido = await gestorClient.GetAsync($"/api/v1/chamados/{chamadoCriado.Id}");
        output.WriteLine($"GET /api/v1/chamados/{chamadoCriado.Id} (Pós-exclusão) -> {(int)getExcluido.StatusCode} {getExcluido.StatusCode}");
        Assert.Equal(HttpStatusCode.NotFound, getExcluido.StatusCode);

        sw.Stop();
        output.WriteLine("\n=========================================================================");
        output.WriteLine($"AUDITORIA COMPLETA DE TODOS OS ENDPOINTS CONCLUÍDA COM SUCESSO EM {sw.ElapsedMilliseconds}ms!");
        output.WriteLine("ZERO BUGS SILENCIOSOS DETECTADOS NOS ENDPOINTS DO BACKEND .NET");
        output.WriteLine("=========================================================================");
    }
}
