using ZelaFix.Domain.Entities;

namespace ZelaFix.Application.Services.Interfaces;

public interface IEnderecoResolver
{
    /// <summary>Consulta o CEP e devolve o endereço, criando estado, cidade e bairro que ainda não existam.</summary>
    /// <exception cref="Domain.Exceptions.DomainException">CEP inexistente.</exception>
    Task<Endereco> ResolverAsync(string cep, string numero, string? referencia, CancellationToken ct = default);
}