using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using ZelaFix.Api.Exceptions;
using ZelaFix.Api.Filters;
using ZelaFix.Api.Security;
using ZelaFix.Application.DTOs;
using ZelaFix.Application.Services.Interfaces;
using ZelaFix.Api.Extensions;

namespace ZelaFix.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/auth").WithTags("Autenticação");

        grupo.MapPost("/register", RegistrarAsync)
            .AllowAnonymous()
            .WithName("Registrar")
            .WithSummary("Cadastra um cidadão (o endereço é resolvido pelo CEP) e já devolve o token JWT.")
            .WithValidation<RegisterRequest>()
            .RequireRateLimiting(RateLimitingExtensions.AuthPolicy)
            .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithName("Login")
            .WithSummary("Autentica com e-mail e senha e devolve o token JWT.")
            .WithValidation<LoginRequest>()
            .RequireRateLimiting(RateLimitingExtensions.AuthPolicy)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        grupo.MapGet("/me", ObterPerfilAsync)
            .RequireAuthorization()
            .WithName("MeuPerfil")
            .WithSummary("Perfil do usuário dono do token.")
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<Created<AuthResponse>> RegistrarAsync(
        RegisterRequest request, IAuthService service, CancellationToken ct)
    {
        var resposta = await service.RegisterAsync(request, ct);
        return TypedResults.Created("/api/v1/auth/me", resposta);
    }

    private static async Task<Results<Ok<AuthResponse>, ProblemHttpResult>> LoginAsync(
        LoginRequest request, IAuthService service, HttpContext http, CancellationToken ct)
    {
        var resposta = await service.LoginAsync(request, ct);
        return resposta is null
            ? TypedResults.Problem(ErroResposta.Criar(
                http, StatusCodes.Status401Unauthorized, "Não autorizado", "E-mail ou senha incorretos."))
            : TypedResults.Ok(resposta);
    }

    private static async Task<Results<Ok<UsuarioResponse>, NotFound>> ObterPerfilAsync(
        ClaimsPrincipal user, IAuthService service, CancellationToken ct) =>
        user.ObterUsuarioId() is { } id && await service.GetPerfilAsync(id, ct) is { } perfil
            ? TypedResults.Ok(perfil)
            : TypedResults.NotFound();
}