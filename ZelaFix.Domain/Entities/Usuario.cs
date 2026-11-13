using ZelaFix.Domain.Common;
using ZelaFix.Domain.Enums;
using ZelaFix.Domain.Exceptions;
using ZelaFix.Domain.Helpers;

namespace ZelaFix.Domain.Entities;

public sealed class Usuario : BaseEntity
{
    public const int TamanhoMinimoSenha = 6;
    public const int TamanhoMaximoSenha = 72; // limite do BCrypt

    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public Cargo Cargo { get; private set; }
    public int EnderecoId { get; private set; }
    public Endereco Endereco { get; private set; } = null!;
    public Telefone? Telefone { get; private set; }
    public List<Chamado> Chamados { get; private set; } = [];

    public bool EhGestor => Cargo == Cargo.Gestor;

    private Usuario() { }

    public Usuario(string nome, string email, string senhaPura, Cargo cargo, int enderecoId)
    {
        AtualizarNome(nome);
        AtualizarEmail(email);
        DefinirSenha(senhaPura);
        AtualizarCargo(cargo);
        AtualizarEndereco(enderecoId);
    }

    public void AtualizarNome(string nome) => Nome = Validacao.Texto(nome, "O nome", 100);

    public void AtualizarEmail(string email)
    {
        email = Validacao.Texto(email, "O e-mail", 100).ToLowerInvariant();
        if (!email.Contains('@'))
            throw new DomainException("O e-mail informado é inválido.");

        Email = email;
    }

    public void DefinirSenha(string senhaPura)
    {
        if (string.IsNullOrWhiteSpace(senhaPura) || senhaPura.Length < TamanhoMinimoSenha)
            throw new DomainException($"A senha deve ter pelo menos {TamanhoMinimoSenha} caracteres.");
        if (senhaPura.Length > TamanhoMaximoSenha)
            throw new DomainException($"A senha deve ter no máximo {TamanhoMaximoSenha} caracteres.");

        SenhaHash = HashHelper.Hash(senhaPura);
    }

    public bool VerificarSenha(string senhaPura) => HashHelper.Verificar(senhaPura, SenhaHash);

    public void AtualizarCargo(Cargo cargo) => Cargo = cargo;

    public void AtualizarEndereco(int enderecoId) =>
        EnderecoId = Validacao.Id(enderecoId, "O usuário deve ter um endereço válido.");
}