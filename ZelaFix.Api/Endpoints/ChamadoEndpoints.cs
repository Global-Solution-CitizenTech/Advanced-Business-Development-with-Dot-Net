using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ZelaFix.Api.Filters;
using ZelaFix.Api.Idempotency;
using ZelaFix.Api.Security;
using ZelaFix.Application.DTOs;
using ZelaFix.Application.Services.Interfaces;

namespace ZelaFix.Api.Endpoints;

public static class ChamadoEndpoints
{
    private const string Rota = "/api/v1/chamados";

    public static IEndpointRouteBuilder MapChamadoEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup(Rota)
            .WithTags("Chamados")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        grupo.MapGet("/", ListarAsync)
            .WithName("ListarChamados")
            .WithSummary("Lista chamados (categoriaId, bairroId, status, busca, apenasMeus, incluirFoto) com paginação (page começa em 0).");

        grupo.MapGet("/{id:int}", ObterAsync)
            .WithName("ObterChamado")
            .WithSummary("Detalha um chamado, com histórico e foto em Base64.");

        grupo.MapPost("/", CriarAsync)
            .WithName("CriarChamado")
            .WithSummary("Abre um chamado (procedure pr_abrir_chamado). Exige o cabeçalho Idempotency-Key.")
            .WithValidation<CriarChamadoRequest>()
            .WithIdempotency<CriarChamadoRequest>();

        grupo.MapPut("/{id:int}", AtualizarAsync)
            .WithName("AtualizarChamado")
            .WithSummary("Edita um chamado (autor enquanto ABERTO, ou gestor).")
            .WithValidation<AtualizarChamadoRequest>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        grupo.MapPatch("/{id:int}/status", AlterarStatusAsync)
            .RequireAuthorization(AuthorizationPolicies.GestorOnly)
            .WithName("AlterarStatusChamado")
            .WithSummary("Altera o status (ABERTO, EM_ANALISE, EM_EXECUCAO, CONCLUIDO, REJEITADO). Somente gestor; exige Idempotency-Key.")
            .WithValidation<AlterarStatusRequest>()
            .WithIdempotency<AlterarStatusRequest>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        grupo.MapPost("/{id:int}/apoiar", ApoiarAsync)
            .WithName("ApoiarChamado")
            .WithSummary("Registra o apoio do usuário (procedure pr_registrar_voto). Repetir não duplica.")
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapDelete("/{id:int}/apoiar", RemoverApoioAsync)
            .WithName("RemoverApoioChamado")
            .WithSummary("Remove o apoio do usuário.");

        grupo.MapDelete("/{id:int}", ExcluirAsync)
            .WithName("ExcluirChamado")
            .WithSummary("Exclui um chamado (autor enquanto ABERTO, ou gestor).")
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<Ok<PaginaResponse<ChamadoResponse>>> ListarAsync(
        [AsParameters] ChamadoFiltro filtro, ClaimsPrincipal user, IChamadoService service, CancellationToken ct) =>
        TypedResults.Ok(await service.ListarAsync(filtro, user.ExigirUsuarioId(), ct));

    private static async Task<Results<Ok<ChamadoResponse>, NotFound>> ObterAsync(
        int id, ClaimsPrincipal user, IChamadoService service, CancellationToken ct) =>
        await service.ObterAsync(id, user.ExigirUsuarioId(), ct) is { } chamado
            ? TypedResults.Ok(chamado)
            : TypedResults.NotFound();

    private static async Task<Created<ChamadoResponse>> CriarAsync(
        CriarChamadoRequest request, ClaimsPrincipal user, IChamadoService service, CancellationToken ct,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null)
    {
        var criado = await service.CriarAsync(request, user.ExigirUsuarioId(), ct);
        return TypedResults.Created($"{Rota}/{criado.Id}", criado);
    }

    private static async Task<Results<Ok<ChamadoResponse>, NotFound>> AtualizarAsync(
        int id, AtualizarChamadoRequest request, ClaimsPrincipal user, IChamadoService service, CancellationToken ct) =>
        await service.AtualizarAsync(id, request, user.ExigirUsuarioId(), user.EhGestor(), ct) is { } chamado
            ? TypedResults.Ok(chamado)
            : TypedResults.NotFound();

    private static async Task<Results<Ok<ChamadoResponse>, NotFound>> AlterarStatusAsync(
        int id, AlterarStatusRequest request, ClaimsPrincipal user, IChamadoService service, CancellationToken ct,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null) =>
        await service.AlterarStatusAsync(id, request, user.ExigirUsuarioId(), ct) is { } chamado
            ? TypedResults.Ok(chamado)
            : TypedResults.NotFound();

    private static async Task<Results<Ok<ChamadoResponse>, NotFound>> ApoiarAsync(
        int id, ClaimsPrincipal user, IChamadoService service, CancellationToken ct) =>
        await service.ApoiarAsync(id, user.ExigirUsuarioId(), ct) is { } chamado
            ? TypedResults.Ok(chamado)
            : TypedResults.NotFound();

    private static async Task<Results<Ok<ChamadoResponse>, NotFound>> RemoverApoioAsync(
        int id, ClaimsPrincipal user, IChamadoService service, CancellationToken ct) =>
        await service.RemoverApoioAsync(id, user.ExigirUsuarioId(), ct) is { } chamado
            ? TypedResults.Ok(chamado)
            : TypedResults.NotFound();

    private static async Task<Results<NoContent, NotFound>> ExcluirAsync(
        int id, ClaimsPrincipal user, IChamadoService service, CancellationToken ct) =>
        await service.ExcluirAsync(id, user.ExigirUsuarioId(), user.EhGestor(), ct)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
}