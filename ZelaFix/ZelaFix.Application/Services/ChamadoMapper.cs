using ZelaFix.Application.DTOs;
using ZelaFix.Application.Media;
using ZelaFix.Domain.Common;
using ZelaFix.Domain.Entities;

namespace ZelaFix.Application.Services;

public static class ChamadoMapper
{
    public static ChamadoResponse Mapear(
        ChamadoLeitura l, IReadOnlyList<HistoricoLeitura>? historico = null, byte[]? foto = null) => new(
        Id: l.Id,
        Protocolo: $"ZEL-{l.DataAbertura.Year}-{l.Id:D4}",
        Titulo: l.Titulo,
        Descricao: l.Descricao,
        CategoriaId: l.CategoriaId,
        Categoria: l.Categoria,
        Status: l.Status ?? Status.Pendente,
        PrioridadeId: l.PrioridadeId,
        Prioridade: l.Prioridade,
        OrgaoId: l.OrgaoId,
        Orgao: l.Orgao,
        ApoiosCount: l.Apoios,
        ApoiadoPeloUsuario: l.ApoiosDoUsuario > 0,
        Localizacao: new LocalizacaoResponse(
            l.Cep, l.Rua, l.Numero,
            l.Referencia == Endereco.SemReferencia ? null : l.Referencia,
            l.Bairro, l.Cidade, l.Estado),
        FotoBase64: foto is { Length: > 0 } ? Convert.ToBase64String(foto) : null,
        FotoContentType: foto is { Length: > 0 } ? FotoImagem.DetectarContentType(foto) : null,
        DataAbertura: Utc(l.DataAbertura),
        DataAtualizacao: Utc(l.UltimaMudanca ?? l.DataAbertura),
        AutorId: l.AutorId,
        AutorNome: l.AutorNome,
        Historico: historico is null ? null : MapearHistorico(historico));

    private static List<HistoricoResponse> MapearHistorico(IReadOnlyList<HistoricoLeitura> historico)
    {
        var lista = new List<HistoricoResponse>(historico.Count);
        for (var i = 0; i < historico.Count; i++)
        {
            var h = historico[i];
            var anterior = i + 1 < historico.Count ? historico[i + 1].Status : null;
            lista.Add(new HistoricoResponse(h.Id, Utc(h.Data), anterior, h.Status, h.Observacoes, h.AutorNome));
        }

        return lista;
    }

    private static DateTime Utc(DateTime data) => DateTime.SpecifyKind(data, DateTimeKind.Utc);
}