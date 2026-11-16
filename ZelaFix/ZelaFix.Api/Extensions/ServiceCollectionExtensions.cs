using Microsoft.OpenApi;
using ZelaFix.Api.Idempotency;
using ZelaFix.Api.Security;
using ZelaFix.Application.Services.Implementations;
using ZelaFix.Application.Services.Interfaces;

namespace ZelaFix.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZelaFixApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEnderecoResolver, EnderecoResolver>();
        services.AddScoped<IChamadoService, ChamadoService>();
        services.AddSingleton<IIdempotencyStore, MemoryIdempotencyStore>();
        
        services.AddHttpClient<IViaCepService, ViaCepService>(client =>
        {
            client.BaseAddress = new Uri("https://viacep.com.br/");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }

    public static IServiceCollection AddZelaFixSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ZelaFix API",
                Version = "v1",
                Description = "API REST de zeladoria urbana colaborativa (FIAP Global Solution 2026/2 - CitizenTech).",
                Contact = new OpenApiContact { Name = "Equipe ZelaFix" }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe o token JWT obtido em POST /api/v1/auth/login."
            });
            options.OperationFilter<BearerSecurityOperationFilter>();
        });

        return services;
    }
}