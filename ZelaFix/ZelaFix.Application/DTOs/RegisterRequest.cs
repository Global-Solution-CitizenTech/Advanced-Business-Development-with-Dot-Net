using System.ComponentModel.DataAnnotations;

namespace ZelaFix.Application.DTOs;

public sealed record RegisterEnderecoInput(string? Cep, string? Numero, string? Referencia, string? Rua);

public sealed record RegisterRequest : IValidatableObject
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    public string Nome { get; init; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail com formato inválido.")]
    [StringLength(100, ErrorMessage = "O e-mail deve ter no máximo 100 caracteres.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(72, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 72 caracteres.")]
    public string Senha { get; init; } = string.Empty;

    public string Telefone { get; init; } = string.Empty;
    public string Cep { get; init; } = string.Empty;
    public string Numero { get; init; } = string.Empty;
    public string? Referencia { get; init; }

    // Compatibilidade com payload do cliente mobile
    public string? Ddd { get; init; }
    public string? NumeroTelefone { get; init; }
    public RegisterEnderecoInput? Endereco { get; init; }

    public string ObterTelefoneResolvido()
    {
        if (!string.IsNullOrWhiteSpace(Telefone)) return Telefone;
        if (!string.IsNullOrWhiteSpace(Ddd) && !string.IsNullOrWhiteSpace(NumeroTelefone))
            return $"({Ddd}) {NumeroTelefone}";
        return string.Empty;
    }

    public string ObterCepResolvido() =>
        !string.IsNullOrWhiteSpace(Cep) ? Cep : (Endereco?.Cep ?? string.Empty);

    public string ObterNumeroResolvido() =>
        !string.IsNullOrWhiteSpace(Numero) ? Numero : (Endereco?.Numero ?? "S/N");

    public string? ObterReferenciaResolvida() =>
        Referencia ?? Endereco?.Referencia;

    /// <summary>Separa DDD e número a partir do telefone digitado (com ou sem máscara).</summary>
    public (string Ddd, string Numero) SepararTelefone()
    {
        var tel = ObterTelefoneResolvido();
        var digitos = new string(tel.Where(char.IsDigit).ToArray());
        return digitos.Length >= 2 ? (digitos[..2], digitos[2..]) : ("11", "999999999");
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var tel = ObterTelefoneResolvido();
        if (string.IsNullOrWhiteSpace(tel))
            yield return new ValidationResult("O telefone é obrigatório.", [nameof(Telefone)]);

        var c = ObterCepResolvido();
        if (string.IsNullOrWhiteSpace(c))
            yield return new ValidationResult("O CEP é obrigatório.", [nameof(Cep)]);

        var n = ObterNumeroResolvido();
        if (string.IsNullOrWhiteSpace(n))
            yield return new ValidationResult("O número do endereço é obrigatório.", [nameof(Numero)]);
    }
}
