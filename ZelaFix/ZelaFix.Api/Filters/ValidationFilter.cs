using System.ComponentModel.DataAnnotations;
using ZelaFix.Api.Exceptions;

namespace ZelaFix.Api.Filters;

/// <summary>Valida o corpo da requisição (DataAnnotations do DTO) antes de executar o endpoint.</summary>
public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        var request = context.Arguments.OfType<T>().FirstOrDefault();

        if (request is null)
            return Results.Problem(ErroResposta.Criar(http, StatusCodes.Status400BadRequest,
                "Requisição inválida", "O corpo da requisição é obrigatório."));

        var resultados = new List<ValidationResult>();
        if (Validator.TryValidateObject(request, new ValidationContext(request), resultados, validateAllProperties: true))
            return await next(context);

        var erros = resultados
            .SelectMany(r => r.MemberNames.DefaultIfEmpty(string.Empty)
                .Select(membro => new ErroCampo(ParaCamelCase(membro), r.ErrorMessage ?? "Valor inválido.")))
            .ToList();

        return Results.Problem(ErroResposta.Criar(http, StatusCodes.Status400BadRequest,
            "Dados inválidos", "Um ou mais campos são inválidos.", erros));
    }

    private static string ParaCamelCase(string nome) =>
        nome.Length == 0 ? nome : char.ToLowerInvariant(nome[0]) + nome[1..];
}

public static class ValidationFilterExtensions
{
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) where T : class =>
        builder.AddEndpointFilter<ValidationFilter<T>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
}