namespace ZelaFix.Application.DTOs;

public sealed record LocalizacaoResponse(
    string Cep, string Logradouro, string Numero, string? PontoReferencia,
    string Bairro, string Cidade, string Estado);

public sealed record HistoricoResponse(
    int Id, DateTime Data, string? StatusAnterior, string NovoStatus, string Observacao, string AutorNome);

/// <summary>A foto trafega em Base64 (FotoBase64 + FotoContentType); não existe URL: a imagem vive no BLOB do banco.</summary>
public sealed record ChamadoResponse(
    int Id, string Protocolo, string Titulo, string Descricao,
    int CategoriaId, string Categoria,
    string Status, string Prioridade,
    int ApoiosCount, bool ApoiadoPeloUsuario,
    LocalizacaoResponse Localizacao,
    string? FotoBase64, string? FotoContentType,
    DateTime DataAbertura, DateTime DataAtualizacao,
    int AutorId, string AutorNome,
    IReadOnlyList<HistoricoResponse>? Historico = null);