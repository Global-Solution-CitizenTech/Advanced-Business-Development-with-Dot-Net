using ZelaFix.Application.Repositories;
using ZelaFix.Application.Services.Interfaces;
using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Application.Services.Implementations;

/// <summary>
/// Resolve Estado → Cidade → Bairro → Endereço a partir do CEP, reaproveitando o que já existe no banco.
/// </summary>
public sealed class EnderecoResolver(
    IViaCepService viaCep,
    IRepository<Estado> estados,
    IRepository<Cidade> cidades,
    IRepository<Bairro> bairros,
    IRepository<Endereco> enderecos) : IEnderecoResolver
{
    private const string NaoInformado = "Não informado";

    public async Task<Endereco> ResolverAsync(
        string cep, string numero, string? referencia, CancellationToken ct = default)
    {
        var cepLimpo = new string(cep.Where(char.IsDigit).ToArray());
        var info = await viaCep.ConsultarAsync(cepLimpo, ct)
            ?? throw new DomainException($"CEP {cepLimpo} não encontrado.");

        var nomeEstado = Texto(info.Estado ?? info.Uf, 30);
        var nomeCidade = Texto(info.Localidade, 50);
        var nomeBairro = Texto(info.Bairro, 50);
        var rua = Texto(info.Logradouro, 100);
        var numeroLimpo = numero.Trim();

        var estadoLower = nomeEstado.ToLowerInvariant();
        var estado = await estados.FirstOrDefaultAsync(e => e.Nome.ToLower() == estadoLower, ct)
            ?? await estados.AddAsync(new Estado(nomeEstado), ct);

        var cidadeLower = nomeCidade.ToLowerInvariant();
        var cidade = await cidades.FirstOrDefaultAsync(
                c => c.Nome.ToLower() == cidadeLower && c.EstadoId == estado.Id, ct)
            ?? await cidades.AddAsync(new Cidade(nomeCidade, estado.Id), ct);

        var bairroLower = nomeBairro.ToLowerInvariant();
        var bairro = await bairros.FirstOrDefaultAsync(
                b => b.Nome.ToLower() == bairroLower && b.CidadeId == cidade.Id, ct)
            ?? await bairros.AddAsync(new Bairro(nomeBairro, cidade.Id), ct);

        return await enderecos.FirstOrDefaultAsync(
                e => e.Cep == cepLimpo && e.Numero == numeroLimpo && e.BairroId == bairro.Id, ct)
            ?? await enderecos.AddAsync(new Endereco(cepLimpo, rua, numeroLimpo, referencia, bairro.Id), ct);
    }

    // CEPs "gerais" de cidades pequenas vêm sem bairro/logradouro; os campos do banco são NOT NULL.
    private static string Texto(string? valor, int maximo)
    {
        var limpo = string.IsNullOrWhiteSpace(valor) ? NaoInformado : valor.Trim();
        return limpo.Length > maximo ? limpo[..maximo] : limpo;
    }
}