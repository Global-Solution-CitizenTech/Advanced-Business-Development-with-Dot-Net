using ZelaFix.Domain.Common;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Domain.Entities;

public sealed class Telefone : BaseEntity
{
    public string Ddd { get; private set; } = string.Empty;
    public string Numero { get; private set; } = string.Empty;
    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;

    public string Completo => $"({Ddd}) {Numero}";

    private Telefone() { }

    public Telefone(string ddd, string numero, int usuarioId)
    {
        Atualizar(ddd, numero);
        UsuarioId = Validacao.Id(usuarioId, "O telefone deve pertencer a um usuário válido.");
    }

    public void Atualizar(string ddd, string numero)
    {
        ddd = (ddd ?? string.Empty).Trim();
        numero = (numero ?? string.Empty).Trim();

        if (ddd.Length != 2 || !ddd.All(char.IsDigit))
            throw new DomainException("O DDD deve ter exatamente 2 dígitos.");
        if (numero.Length is < 8 or > 9 || !numero.All(char.IsDigit))
            throw new DomainException("O número de telefone deve ter 8 ou 9 dígitos.");

        Ddd = ddd;
        Numero = numero;
    }
}