using System.ComponentModel.DataAnnotations;

namespace ZelaFix.Application.DTOs;

public sealed record RegisterRequest
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

    [Required(ErrorMessage = "O telefone é obrigatório.")]
    [RegularExpression(@"^\(?\d{2}\)?\s?\d{4,5}-?\d{4}$",
        ErrorMessage = "Informe o telefone com DDD, por exemplo (11) 98765-4321.")]
    public string Telefone { get; init; } = string.Empty;

    [Required(ErrorMessage = "O CEP é obrigatório.")]
    [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "O CEP deve estar no formato 00000-000.")]
    public string Cep { get; init; } = string.Empty;

    [Required(ErrorMessage = "O número do endereço é obrigatório.")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "O número deve ter entre 1 e 10 caracteres.")]
    public string Numero { get; init; } = string.Empty;

    [StringLength(50, ErrorMessage = "A referência deve ter no máximo 50 caracteres.")]
    public string? Referencia { get; init; }

    /// <summary>Separa DDD e número a partir do telefone digitado (com ou sem máscara).</summary>
    public (string Ddd, string Numero) SepararTelefone()
    {
        var digitos = new string(Telefone.Where(char.IsDigit).ToArray());
        return (digitos[..2], digitos[2..]);
    }
}