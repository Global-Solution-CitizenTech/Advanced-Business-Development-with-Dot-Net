using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ZelaFix.Api.Security;

/// <summary>Mostra o cadeado do Swagger só nos endpoints que exigem autenticação.</summary>
public sealed class BearerSecurityOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        var exigeAutenticacao = metadata.OfType<IAuthorizeData>().Any();
        var anonimo = metadata.OfType<IAllowAnonymous>().Any();

        if (!exigeAutenticacao || anonimo)
            return;

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
            }
        ];
    }
}