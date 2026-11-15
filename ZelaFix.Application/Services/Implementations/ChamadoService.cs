using Microsoft.Extensions.Logging;
using ZelaFix.Application.DTOs;
using ZelaFix.Application.Exceptions;
using ZelaFix.Application.Media;
using ZelaFix.Application.Repositories;
using ZelaFix.Application.Services.Interfaces;
using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Application.Services.Implementations;

public sealed class ChamadoService(
    IChamadoRepository chamados,
    IRepository<Categoria> categorias,
    IRepository<Prioridade> prioridades,
    IRepository<OrgaoGoverno> orgaos,
    IRepository<HistChamado> historicos,
    IRepository<Status> statusRepository,
    IRepository<VotoChamado> votos,
    IChamadoProcedures procedures,
    IEnderecoResolver enderecoResolver,
    IUnitOfWork unitOfWork,
    ILogger<ChamadoService> logger) : IChamadoService
{
    public async Task<PaginaResponse<ChamadoResponse>> ListarAsync(
        ChamadoFiltro filtro, int usuarioId, CancellationToken ct = default)
    {
        var f = filtro.Normalizado();
        if (f.Status is not null)
            f.Status = Status.Canonico(f.Status);

        var pagina = await chamados.BuscarAsync(f, usuarioId, ct);

        var fotos = f.IncluirFoto == true
            ? await chamados.ObterFotosAsync(pagina.Content.Select(c => c.Id).ToList(), ct)
            : null;

        return pagina.Map(l => ChamadoMapper.Mapear(l, null, fotos?.GetValueOrDefault(l.Id)));
    }

    public async Task<ChamadoResponse?> ObterAsync(int id, int usuarioId, CancellationToken ct = default)
    {
        var leitura = await chamados.ObterLeituraAsync(id, usuarioId, ct);
        if (leitura is null)
            return null;

        var historico = await chamados.ObterHistoricoAsync(id, ct);
        var foto = await chamados.GetFotoAsync(id, ct);
        return ChamadoMapper.Mapear(leitura, historico, foto);
    }

    public async Task<ChamadoResponse> CriarAsync(CriarChamadoRequest request, int usuarioId, CancellationToken ct = default)
    {
        var foto = FotoImagem.Decodificar(request.FotoBase64);
        var categoria = await GarantirCategoriaAsync(request.CategoriaId, ct);

        // Prioridade: se informada e válida usa; senão default 2 (MEDIA)
        var prioridadeId = request.PrioridadeId.HasValue && request.PrioridadeId.Value > 0
            && await prioridades.ExistsAsync(p => p.Id == request.PrioridadeId.Value, ct)
                ? request.PrioridadeId.Value
                : (await prioridades.FirstOrDefaultAsync(p => p.Nome == "MEDIA", ct))?.Id ?? 2;

        // Órgão governamental: se informado e válido usa; senão mapeia inteligentemente pela categoria
        var orgaoId = request.OrgaoGovernoId.HasValue && request.OrgaoGovernoId.Value > 0
            && await orgaos.ExistsAsync(o => o.Id == request.OrgaoGovernoId.Value, ct)
                ? request.OrgaoGovernoId.Value
                : await MapearOrgaoPorCategoriaAsync(categoria.Nome, ct);

        var id = await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var endereco = await enderecoResolver.ResolverAsync(request.Cep, request.Numero, request.Referencia, ct);
            return await procedures.AbrirChamadoAsync(
                request.Titulo.Trim(), request.Descricao.Trim(), foto.Bytes,
                usuarioId, endereco.Id, request.CategoriaId, prioridadeId, orgaoId, ct);
        }, ct);

        logger.LogInformation("Chamado {ChamadoId} aberto pelo cidadão {UsuarioId}.", id, usuarioId);
        return (await ObterAsync(id, usuarioId, ct))!;
    }

    public async Task<ChamadoResponse?> AtualizarAsync(
        int id, AtualizarChamadoRequest request, int usuarioId, bool ehGestor, CancellationToken ct = default)
    {
        var atual = await chamados.ObterLeituraAsync(id, usuarioId, ct);
        if (atual is null)
            return null;

        if (!ehGestor && atual.AutorId != usuarioId)
            throw new AcessoNegadoException("Somente o autor ou um gestor pode editar este chamado.");
        if (!ehGestor && (atual.Status ?? Status.Pendente) != Status.Pendente)
            throw new DomainException("O chamado só pode ser editado pelo autor enquanto estiver 'PENDENTE'.");

        await GarantirCategoriaAsync(request.CategoriaId, ct);
        var novaFoto = string.IsNullOrWhiteSpace(request.FotoBase64)
            ? null
            : FotoImagem.Decodificar(request.FotoBase64).Bytes;

        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var endereco = await enderecoResolver.ResolverAsync(request.Cep, request.Numero, request.Referencia, ct);
            var chamado = await chamados.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Chamado não encontrado.");

            chamado.Atualizar(request.Titulo, request.Descricao, novaFoto ?? chamado.Foto, endereco.Id, request.CategoriaId);
            await chamados.UpdateAsync(chamado, ct);
            return true;
        }, ct);

        logger.LogInformation("Chamado {ChamadoId} atualizado por {UsuarioId}.", id, usuarioId);
        return await ObterAsync(id, usuarioId, ct);
    }

    public async Task<ChamadoResponse?> AlterarStatusAsync(
        int id, AlterarStatusRequest request, int gestorId, CancellationToken ct = default)
    {
        var novo = Status.Canonico(request.Status);

        var atual = await chamados.ObterLeituraAsync(id, gestorId, ct);
        if (atual is null)
            return null;

        var statusAtual = atual.Status ?? Status.Pendente;
        if (statusAtual == novo)
            throw new ConflitoException($"O chamado já está com o status '{novo}'.");

        var observacao = string.IsNullOrWhiteSpace(request.Observacao)
            ? $"Status alterado de {statusAtual} para {novo}."
            : request.Observacao.Trim();

        var status = await ObterStatusAsync(novo, ct);
        await procedures.AtualizarStatusAsync(id, status.Id, gestorId, observacao, ct);

        logger.LogInformation("Chamado {ChamadoId} movido de {De} para {Para} pelo gestor {GestorId}.",
            id, statusAtual, novo, gestorId);
        return await ObterAsync(id, gestorId, ct);
    }

    public async Task<ChamadoResponse?> ApoiarAsync(int id, int usuarioId, CancellationToken ct = default)
    {
        var atual = await chamados.ObterLeituraAsync(id, usuarioId, ct);
        if (atual is null)
            return null;

        // Regra de Integridade Cívica: Autor não pode apoiar o próprio chamado
        if (atual.AutorId == usuarioId)
            throw new AcessoNegadoException("O autor não pode apoiar o próprio chamado.");

        // Idempotência do voto: apoiar mais de uma vez não duplica
        if (!await votos.ExistsAsync(v => v.ChamadoId == id && v.UsuarioId == usuarioId, ct))
        {
            await procedures.RegistrarVotoAsync(id, usuarioId, ct);
            logger.LogInformation("Usuário {UsuarioId} apoiou o chamado {ChamadoId}.", usuarioId, id);

            // Regra de Negócio: Escalonamento Comunitário de Prioridade por Apoios Populares
            var totalVotos = await votos.CountAsync(v => v.ChamadoId == id, ct) + 1;
            var chamado = await chamados.GetByIdAsync(id, ct);
            if (chamado is not null && atual.Status != Status.Concluido && atual.Status != Status.Rejeitado)
            {
                int? novaPrioridadeId = totalVotos switch
                {
                    >= 5 when chamado.PrioridadeId < 4 => 4, // URGENTE
                    >= 3 when chamado.PrioridadeId < 3 => 3, // ALTA
                    _ => null
                };

                if (novaPrioridadeId is { } novaP)
                {
                    await unitOfWork.ExecuteInTransactionAsync(async () =>
                    {
                        chamado.AtualizarPrioridade(novaP);
                        await chamados.UpdateAsync(chamado, ct);
                        var statusAtual = await ObterStatusAsync(atual.Status ?? Status.Pendente, ct);
                        await historicos.AddAsync(new HistChamado(
                            chamado.Id,
                            statusAtual.Id,
                            usuarioId,
                            $"Escalonamento comunitário: {totalVotos} apoios populares atingidos."), ct);
                        return true;
                    }, ct);
                    logger.LogInformation("Chamado {ChamadoId} escalonado para prioridade {NovaPrioridade} após {Total} apoios.", id, novaP, totalVotos);
                }
            }
        }

        return await ObterAsync(id, usuarioId, ct);
    }

    public async Task<ChamadoResponse?> RemoverApoioAsync(int id, int usuarioId, CancellationToken ct = default)
    {
        if (!await chamados.ExistsAsync(c => c.Id == id, ct))
            return null;

        var voto = await votos.FirstOrDefaultAsync(v => v.ChamadoId == id && v.UsuarioId == usuarioId, ct);
        if (voto is not null)
        {
            await votos.DeleteAsync(voto.Id, ct);
            logger.LogInformation("Usuário {UsuarioId} removeu o apoio ao chamado {ChamadoId}.", usuarioId, id);
        }

        return await ObterAsync(id, usuarioId, ct);
    }

    public async Task<bool> ExcluirAsync(int id, int usuarioId, bool ehGestor, CancellationToken ct = default)
    {
        var atual = await chamados.ObterLeituraAsync(id, usuarioId, ct);
        if (atual is null)
            return false;

        if (!ehGestor)
        {
            if (atual.AutorId != usuarioId)
                throw new AcessoNegadoException("Somente o autor ou um gestor pode excluir este chamado.");
            if ((atual.Status ?? Status.Pendente) != Status.Pendente)
                throw new DomainException("O chamado só pode ser excluído pelo autor enquanto estiver 'PENDENTE'.");
        }

        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await chamados.ExcluirComDependenciasAsync(id, ct);
            return true;
        }, ct);

        logger.LogInformation("Chamado {ChamadoId} excluído por {UsuarioId}.", id, usuarioId);
        return true;
    }

    private async Task<Categoria> GarantirCategoriaAsync(int categoriaId, CancellationToken ct) =>
        await categorias.GetByIdAsync(categoriaId, ct)
        ?? throw new DomainException("Categoria não encontrada.");

    private async Task<int> MapearOrgaoPorCategoriaAsync(string categoriaNome, CancellationToken ct)
    {
        var nomeOrgao = categoriaNome switch
        {
            "ILUMINACAO_PUBLICA" => "ILUMINACAO_PUBLICA",
            "VIAS_E_PAVIMENTACAO" => "SECRETARIA_OBRAS",
            "DRENAGEM_E_ESGOTO" => "SANEAMENTO_BASICO",
            "LIMPEZA_URBANA" => "SUBPREFEITURA",
            "ARVORES_E_PARQUES" => "SUBPREFEITURA",
            "SINALIZACAO_E_TRANSITO" => "CET_TRANSITO",
            _ => "SUBPREFEITURA"
        };

        var orgao = await orgaos.FirstOrDefaultAsync(o => o.Nome == nomeOrgao, ct);
        return orgao?.Id ?? 1;
    }

    private async Task<Status> ObterStatusAsync(string nome, CancellationToken ct) =>
        await statusRepository.FirstOrDefaultAsync(s => s.Nome == nome, ct)
        ?? await statusRepository.AddAsync(new Status(nome), ct);
}
