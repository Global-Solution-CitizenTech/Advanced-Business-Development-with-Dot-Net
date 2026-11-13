using ZelaFix.Application.Repositories;

namespace ZelaFix.Api.Endpoints;

public static class EngajamentoEndpoints
{
    public static IEndpointRouteBuilder MapEngajamentoEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/bairros/{bairroId:int}")
            .WithTags("Engajamento")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        grupo.MapGet("/criticidade", async (int bairroId, IChamadoProcedures procedures, CancellationToken ct) =>
                Results.Ok(new { bairroId, criticidade = await procedures.ClassificarCriticidadeBairroAsync(bairroId, ct) }))
            .WithName("CriticidadeBairro")
            .WithSummary("Classifica a criticidade do bairro (função do banco).");

        grupo.MapGet("/engajamento", async (int bairroId, IChamadoProcedures procedures, CancellationToken ct) =>
                Results.Content(await procedures.RelatorioEngajamentoJsonAsync(bairroId, ct), "application/json"))
            .WithName("EngajamentoBairro")
            .WithSummary("Relatório de engajamento do bairro em JSON (procedure do banco).")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}