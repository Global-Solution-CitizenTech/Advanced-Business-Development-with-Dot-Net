using System.ComponentModel.DataAnnotations;

namespace ZelaFix.Application.DTOs;

public sealed record LoginRequest
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail com formato inválido.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    public string Senha { get; init; } = string.Empty;
}