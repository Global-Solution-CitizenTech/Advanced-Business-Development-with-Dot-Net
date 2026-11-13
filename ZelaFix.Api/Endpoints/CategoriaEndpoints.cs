using Microsoft.AspNetCore.Http.HttpResults;
using ZelaFix.Api.Filters;
using ZelaFix.Api.Security;
using ZelaFix.Application.DTOs;
using ZelaFix.Application.Services.Interfaces;

namespace ZelaFix.Api.Endpoints;

public static class CategoriaEndpoints
{
    private const string Rota = "/api/v1/categorias";

    public static IEndpointRouteBuilder MapCategoriaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup(Rota)
            .WithTags("Categorias")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        grupo.MapGet("/", ListarAsync)
            .WithName("ListarCategorias")
            .WithSummary("Lista todas as categorias de chamado.");

        grupo.MapGet("/{id:int}", ObterAsync)
            .WithName("ObterCategoria")
            .WithSummary("Obtém uma categoria pelo id.");

        grupo.MapPost("/", CriarAsync)
            .RequireAuthorization(AuthorizationPolicies.GestorOnly)
            .WithName("CriarCategoria")
            .WithSummary("Cadastra uma nova categoria (somente gestor).")
            .WithValidation<CategoriaRequest>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapPut("/{id:int}", AtualizarAsync)
            .RequireAuthorization(AuthorizationPolicies.GestorOnly)
            .WithName("AtualizarCategoria")
            .WithSummary("Atualiza o nome de uma categoria (somente gestor).")
            .WithValidation<CategoriaRequest>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapDelete("/{id:int}", RemoverAsync)
            .RequireAuthorization(AuthorizationPolicies.GestorOnly)
            .WithName("RemoverCategoria")
            .WithSummary("Remove uma categoria (somente gestor; 409 se houver chamados vinculados).")
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<CategoriaResponse>>> ListarAsync(
        ICategoriaService service, CancellationToken ct) =>
        TypedResults.Ok(await service.GetAllAsync(ct));

    private static async Task<Results<Ok<CategoriaResponse>, NotFound>> ObterAsync(
        int id, ICategoriaService service, CancellationToken ct) =>
        await service.GetByIdAsync(id, ct) is { } categoria
            ? TypedResults.Ok(categoria)
            : TypedResults.NotFound();

    private static async Task<Created<CategoriaResponse>> CriarAsync(
        CategoriaRequest request, ICategoriaService service, CancellationToken ct)
    {
        var criada = await service.CreateAsync(request, ct);
        return TypedResults.Created($"{Rota}/{criada.Id}", criada);
    }

    private static async Task<Results<Ok<CategoriaResponse>, NotFound>> AtualizarAsync(
        int id, CategoriaRequest request, ICategoriaService service, CancellationToken ct) =>
        await service.UpdateAsync(id, request, ct) is { } categoria
            ? TypedResults.Ok(categoria)
            : TypedResults.NotFound();

    private static async Task<Results<NoContent, NotFound>> RemoverAsync(
        int id, ICategoriaService service, CancellationToken ct) =>
        await service.DeleteAsync(id, ct)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
}