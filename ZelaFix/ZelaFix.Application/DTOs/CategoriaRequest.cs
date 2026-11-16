using System.ComponentModel.DataAnnotations;

namespace ZelaFix.Application.DTOs;

public sealed record CategoriaRequest
{
    [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 50 caracteres.")]
    public string Nome { get; init; } = string.Empty;
}