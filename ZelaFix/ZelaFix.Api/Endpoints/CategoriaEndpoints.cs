using Microsoft.AspNetCore.Http.HttpResults;
using ZelaFix.Api.Filters;
using ZelaFix.Api.Hateoas;
using ZelaFix.Api.Security;
using ZelaFix.Application.DTOs;
using ZelaFix.Application.Services.Interfaces;

namespace ZelaFix.Api.Endpoints;

public static class CategoriaEndpoints
{
    public static IEndpointRouteBuilder MapCategoriaEndpoints(this IEndpointRouteBuilder app)
    {
        MapearRotas(app.MapGroup("/api/v1/categorias").WithTags("Categorias"));
        MapearRotas(app.MapGroup("/categorias").ExcludeFromDescription());
        return app;
    }

    private static void MapearRotas(RouteGroupBuilder grupo)
    {
        grupo.RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        grupo.MapGet("/", ListarAsync)
            .WithSummary("Lista todas as categorias de chamado.");

        grupo.MapGet("/{id:int}", ObterAsync)
            .WithSummary("Obtém uma categoria pelo id.");

        grupo.MapPost("/", CriarAsync)
            .RequireAuthorization(AuthorizationPolicies.GestorOnly)
            .WithSummary("Cadastra uma nova categoria (somente gestor).")
            .WithValidation<CategoriaRequest>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapPut("/{id:int}", AtualizarAsync)
            .RequireAuthorization(AuthorizationPolicies.GestorOnly)
            .WithSummary("Atualiza o nome de uma categoria (somente gestor).")
            .WithValidation<CategoriaRequest>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapDelete("/{id:int}", RemoverAsync)
            .RequireAuthorization(AuthorizationPolicies.GestorOnly)
            .WithSummary("Remove uma categoria (somente gestor; 409 se houver chamados vinculados).")
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<Ok<IReadOnlyList<CategoriaResponse>>> ListarAsync(
        ICategoriaService service, CancellationToken ct) =>
        TypedResults.Ok(ResourceLinks.EnrichAll(await service.GetAllAsync(ct)));

    private static async Task<Results<Ok<CategoriaResponse>, NotFound>> ObterAsync(
        int id, ICategoriaService service, CancellationToken ct) =>
        await service.GetByIdAsync(id, ct) is { } c
            ? TypedResults.Ok(ResourceLinks.Enrich(c))
            : TypedResults.NotFound();

    private static async Task<Results<Created<CategoriaResponse>, Conflict>> CriarAsync(
        CategoriaRequest request, ICategoriaService service, CancellationToken ct)
    {
        var criada = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/v1/categorias/{criada.Id}", ResourceLinks.Enrich(criada));
    }

    private static async Task<Results<Ok<CategoriaResponse>, NotFound, Conflict>> AtualizarAsync(
        int id, CategoriaRequest request, ICategoriaService service, CancellationToken ct)
    {
        var atualizada = await service.UpdateAsync(id, request, ct);
        return atualizada is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ResourceLinks.Enrich(atualizada));
    }

    private static async Task<Results<NoContent, NotFound>> RemoverAsync(
        int id, ICategoriaService service, CancellationToken ct)
    {
        var removida = await service.DeleteAsync(id, ct);
        return removida ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
