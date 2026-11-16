using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ZelaFix.Api.Filters;
using ZelaFix.Api.Hateoas;
using ZelaFix.Api.Idempotency;
using ZelaFix.Api.Security;
using ZelaFix.Application.DTOs;
using ZelaFix.Application.Services.Interfaces;

namespace ZelaFix.Api.Endpoints;

public static class ChamadoEndpoints
{
    public static IEndpointRouteBuilder MapChamadoEndpoints(this IEndpointRouteBuilder app)
    {
        MapearRotas(app.MapGroup("/api/v1/chamados").WithTags("Chamados"));
        MapearRotas(app.MapGroup("/chamados").ExcludeFromDescription());
        return app;
    }

    private static void MapearRotas(RouteGroupBuilder grupo)
    {
        grupo.RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        grupo.MapGet("/", ListarAsync)
            .WithSummary("Lista chamados (categoriaId, bairroId, status, busca, apenasMeus, incluirFoto) com paginação (page começa em 0).");

        grupo.MapGet("/estatisticas", ObterEstatisticasAsync)
            .WithSummary("Retorna o total consolidado de chamados por status e total de apoios.");

        grupo.MapGet("/{id:int}", ObterAsync)
            .WithSummary("Detalha um chamado, com histórico e foto em Base64.");

        grupo.MapPost("/", CriarAsync)
            .WithSummary("Abre um novo chamado com título, descrição, foto, categoria e endereço. Exige Idempotency-Key.")
            .WithValidation<CriarChamadoRequest>()
            .WithIdempotency<CriarChamadoRequest>();

        grupo.MapPut("/{id:int}", AtualizarAsync)
            .WithSummary("Edita um chamado (autor enquanto PENDENTE, ou gestor).")
            .WithValidation<AtualizarChamadoRequest>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        grupo.MapPatch("/{id:int}/status", AlterarStatusAsync)
            .RequireAuthorization(AuthorizationPolicies.GestorOnly)
            .WithSummary("Altera o status (PENDENTE, EM_ANALISE, EM_EXECUCAO, CONCLUIDO, REJEITADO). Somente gestor; exige Idempotency-Key.")
            .WithValidation<AlterarStatusRequest>()
            .WithIdempotency<AlterarStatusRequest>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        grupo.MapPost("/{id:int}/apoiar", ApoiarAsync)
            .WithSummary("Registra o apoio do usuário ao chamado. Repetir não duplica.")
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapDelete("/{id:int}/apoiar", RemoverApoioAsync)
            .WithSummary("Remove o apoio do usuário.");

        grupo.MapDelete("/{id:int}", ExcluirAsync)
            .WithSummary("Exclui um chamado (autor enquanto PENDENTE, ou gestor).")
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static async Task<Ok<PaginaResponse<ChamadoResponse>>> ListarAsync(
        [AsParameters] ChamadoFiltro filtro, ClaimsPrincipal user, IChamadoService service, CancellationToken ct) =>
        TypedResults.Ok(ResourceLinks.Enrich(await service.ListarAsync(filtro, user.ObterUsuarioId() ?? 0, ct)));

    private static async Task<Ok<EstatisticasResponse>> ObterEstatisticasAsync(
        IChamadoService service, CancellationToken ct) =>
        TypedResults.Ok(await service.ObterEstatisticasAsync(ct));

    private static async Task<Results<Ok<ChamadoResponse>, NotFound>> ObterAsync(
        int id, ClaimsPrincipal user, IChamadoService service, CancellationToken ct) =>
        await service.ObterAsync(id, user.ObterUsuarioId() ?? 0, ct) is { } c
            ? TypedResults.Ok(ResourceLinks.Enrich(c))
            : TypedResults.NotFound();

    private static async Task<Results<Created<ChamadoResponse>, BadRequest>> CriarAsync(
        [FromBody] CriarChamadoRequest request, ClaimsPrincipal user, IChamadoService service, CancellationToken ct)
    {
        if (user.ObterUsuarioId() is not { } usuarioId)
            return TypedResults.BadRequest();

        var criado = await service.CriarAsync(request, usuarioId, ct);
        return TypedResults.Created($"/api/v1/chamados/{criado.Id}", ResourceLinks.Enrich(criado));
    }

    private static async Task<Results<Ok<ChamadoResponse>, NotFound, ForbidHttpResult>> AtualizarAsync(
        int id, [FromBody] AtualizarChamadoRequest request, ClaimsPrincipal user, IChamadoService service, CancellationToken ct)
    {
        if (user.ObterUsuarioId() is not { } usuarioId)
            return TypedResults.Forbid();

        var atualizado = await service.AtualizarAsync(id, request, usuarioId, user.EhGestor(), ct);
        return atualizado is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ResourceLinks.Enrich(atualizado));
    }

    private static async Task<Results<Ok<ChamadoResponse>, NotFound, ForbidHttpResult>> AlterarStatusAsync(
        int id, [FromBody] AlterarStatusRequest request, ClaimsPrincipal user, IChamadoService service, CancellationToken ct)
    {
        if (user.ObterUsuarioId() is not { } gestorId)
            return TypedResults.Forbid();

        var atualizado = await service.AlterarStatusAsync(id, request, gestorId, ct);
        return atualizado is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ResourceLinks.Enrich(atualizado));
    }

    private static async Task<Results<Ok<ChamadoResponse>, NotFound>> ApoiarAsync(
        int id, ClaimsPrincipal user, IChamadoService service, CancellationToken ct)
    {
        var atualizado = await service.ApoiarAsync(id, user.ObterUsuarioId() ?? 0, ct);
        return atualizado is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ResourceLinks.Enrich(atualizado));
    }

    private static async Task<Results<Ok<ChamadoResponse>, NotFound>> RemoverApoioAsync(
        int id, ClaimsPrincipal user, IChamadoService service, CancellationToken ct)
    {
        var atualizado = await service.RemoverApoioAsync(id, user.ObterUsuarioId() ?? 0, ct);
        return atualizado is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ResourceLinks.Enrich(atualizado));
    }

    private static async Task<Results<NoContent, NotFound, ForbidHttpResult>> ExcluirAsync(
        int id, ClaimsPrincipal user, IChamadoService service, CancellationToken ct)
    {
        if (user.ObterUsuarioId() is not { } usuarioId)
            return TypedResults.Forbid();

        var removido = await service.ExcluirAsync(id, usuarioId, user.EhGestor(), ct);
        return removido ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
