using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ZelaFix.Api.Health;

/// <summary>Registra os endpoints de Health Checks (/health, /health/ready, /health/live) na especificação OpenAPI/Swagger.</summary>
public sealed class HealthChecksDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Paths ??= new OpenApiPaths();

        AddHealthPath(swaggerDoc, "/health", "Diagnóstico completo de integridade",
            "Verifica o processo da API, conectividade com o Oracle Database e disponibilidade do serviço externo ViaCEP.");

        AddHealthPath(swaggerDoc, "/health/ready", "Readiness Probe",
            "Verifica se as dependências críticas (Oracle Database e ViaCEP) estão prontas para receber tráfego.");

        AddHealthPath(swaggerDoc, "/health/live", "Liveness Probe",
            "Verifica se o processo da API está ativo e respondendo.");
    }

    private static void AddHealthPath(OpenApiDocument swaggerDoc, string route, string summary, string description)
    {
        var pathItem = new OpenApiPathItem
        {
            Operations = new Dictionary<HttpMethod, OpenApiOperation>
            {
                [HttpMethod.Get] = new OpenApiOperation
                {
                    Tags = new HashSet<OpenApiTagReference> { new("Monitoramento & Health") },
                    Summary = summary,
                    Description = description,
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse { Description = "Healthy — Todos os componentes verificados estão operacionais." },
                        ["503"] = new OpenApiResponse { Description = "Unhealthy — Uma ou mais dependências estão indisponíveis." }
                    }
                }
            }
        };

        swaggerDoc.Paths[route] = pathItem;
    }
}
