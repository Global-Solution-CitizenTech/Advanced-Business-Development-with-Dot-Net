using ZelaFix.Domain.Common;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Domain.Entities;

public sealed class Endereco : BaseEntity
{
    public const string SemReferencia = "Sem referência";

    public string Cep { get; private set; } = string.Empty;
    public string Rua { get; private set; } = string.Empty;
    public string Numero { get; private set; } = string.Empty;
    public string Referencia { get; private set; } = SemReferencia;
    public int BairroId { get; private set; }
    public Bairro Bairro { get; private set; } = null!;

    private Endereco() { }

    public Endereco(string cep, string rua, string numero, string? referencia, int bairroId) =>
        Atualizar(cep, rua, numero, referencia, bairroId);

    public void Atualizar(string cep, string rua, string numero, string? referencia, int bairroId)
    {
        var digitos = new string((cep ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digitos.Length != 8)
            throw new DomainException("O CEP deve ter 8 dígitos.");

        Cep = digitos;
        Rua = Validacao.Texto(rua, "A rua", 100);
        Numero = Validacao.Texto(numero, "O número", 100);

        // Oracle trata string vazia como NULL e REFERENCIA é NOT NULL.
        Referencia = string.IsNullOrWhiteSpace(referencia)
            ? SemReferencia
            : Validacao.Texto(referencia, "A referência", 50);

        BairroId = Validacao.Id(bairroId, "O endereço deve estar associado a um bairro válido.");
    }
}