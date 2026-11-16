using System.ComponentModel.DataAnnotations;

namespace ZelaFix.Application.DTOs;

public abstract record ChamadoBaseRequest
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "O título deve ter entre 5 e 100 caracteres.")]
    public string Titulo { get; init; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre 10 e 1000 caracteres.")]
    public string Descricao { get; init; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Informe uma categoria válida.")]
    public int CategoriaId { get; init; }

    [Required(ErrorMessage = "O CEP é obrigatório.")]
    [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "O CEP deve estar no formato 00000-000.")]
    public string Cep { get; init; } = string.Empty;

    [Required(ErrorMessage = "O número é obrigatório.")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "O número deve ter entre 1 e 10 caracteres.")]
    public string Numero { get; init; } = string.Empty;

    [StringLength(50, ErrorMessage = "A referência deve ter no máximo 50 caracteres.")]
    public string? Referencia { get; init; }

    /// <summary>Opcional: 1 (BAIXA), 2 (MEDIA), 3 (ALTA), 4 (URGENTE). Padrão: 2 (MEDIA).</summary>
    [Range(1, 4, ErrorMessage = "A prioridade deve estar entre 1 (BAIXA) e 4 (URGENTE).")]
    public int? PrioridadeId { get; init; }

    /// <summary>Opcional: Órgão governamental responsável. Se omitido, é atribuído automaticamente pela categoria.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Informe um órgão válido.")]
    public int? OrgaoGovernoId { get; init; }
}

public sealed record CriarChamadoRequest : ChamadoBaseRequest
{
    /// <summary>Imagem em Base64 (JPEG, PNG ou WebP, até 5 MB). Aceita o prefixo "data:image/...;base64,".</summary>
    [Required(ErrorMessage = "A foto é obrigatória.")]
    [StringLength(7_000_000, ErrorMessage = "A foto excede o tamanho máximo permitido.")]
    public string FotoBase64 { get; init; } = string.Empty;
}

public sealed record AtualizarChamadoRequest : ChamadoBaseRequest
{
    /// <summary>Opcional: se omitida, a foto atual é mantida.</summary>
    [StringLength(7_000_000, ErrorMessage = "A foto excede o tamanho máximo permitido.")]
    public string? FotoBase64 { get; init; }
}

public sealed record AlterarStatusRequest
{
    [Required(ErrorMessage = "O status é obrigatório.")]
    public string Status { get; init; } = string.Empty;

    [StringLength(255, ErrorMessage = "A observação deve ter no máximo 255 caracteres.")]
    public string? Observacao { get; init; }
}
