using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ZelaFix.Api.Exceptions;
using ZelaFix.Api.Security;
using ZelaFix.Application.Auth;
using ZelaFix.Application.Services.Implementations;
using ZelaFix.Application.Services.Interfaces;
using ZelaFix.Domain.Enums;

namespace ZelaFix.Api.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddZelaFixJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
        settings.Validar(); // falha na inicialização se a chave não estiver configurada

        services.AddSingleton(settings);
        services.AddSingleton<ITokenService, TokenService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ClockSkew = TimeSpan.FromSeconds(30),
                    RoleClaimType = ClaimsNomes.Perfil,
                    NameClaimType = ClaimsNomes.Nome
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/problem+json";
                        var problem = ErroResposta.Criar(
                            context.HttpContext,
                            StatusCodes.Status401Unauthorized,
                            "Não autorizado",
                            "Token JWT ausente ou inválido.");
                        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/problem+json";
                        var problem = ErroResposta.Criar(
                            context.HttpContext,
                            StatusCodes.Status403Forbidden,
                            "Acesso negado",
                            "Usuário autenticado não possui permissão para acessar este recurso.");
                        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.GestorOnly,
                policy => policy.RequireRole(Cargo.Gestor.ParaPerfil(), Cargo.Admin.ParaPerfil()));

        return services;
    }
}