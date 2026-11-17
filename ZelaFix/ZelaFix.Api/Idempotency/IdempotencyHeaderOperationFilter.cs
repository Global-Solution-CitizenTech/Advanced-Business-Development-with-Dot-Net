using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ZelaFix.Api.Idempotency;

/// <summary>Adiciona o cabeçalho obrigatório Idempotency-Key no Swagger nos endpoints idempotentes.</summary>
public sealed class IdempotencyHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        if (!metadata.OfType<RequiresIdempotencyKeyMetadata>().Any())
            return;

        operation.Parameters ??= [];
        if (operation.Parameters.Any(p => string.Equals(p.Name, IdempotencyFilter<object>.Cabecalho, StringComparison.OrdinalIgnoreCase)))
            return;

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = IdempotencyFilter<object>.Cabecalho,
            In = ParameterLocation.Header,
            Required = true,
            Description = "Chave única de idempotência por tentativa de operação (ex.: UUID v4).",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        });
    }
}
