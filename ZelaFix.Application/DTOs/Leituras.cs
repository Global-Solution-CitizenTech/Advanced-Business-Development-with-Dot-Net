namespace ZelaFix.Application.DTOs;

public sealed record ChamadoLeitura(
    int Id, string Titulo, string Descricao,
    int CategoriaId, string Categoria,
    int PrioridadeId, string Prioridade,
    int OrgaoId, string Orgao,
    int AutorId, string AutorNome,
    string? Status, int Apoios, int ApoiosDoUsuario,
    string Cep, string Rua, string Numero, string Referencia,
    string Bairro, string Cidade, string Estado,
    DateTime DataAbertura, DateTime? UltimaMudanca);

public sealed record HistoricoLeitura(int Id, DateTime Data, string Status, string Observacoes, string AutorNome);