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

        // Fotos só vêm na listagem se o cliente pedir (incluirFoto=true): é o BLOB, o payload pesado.
        var fotos = f.IncluirFoto
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
        await GarantirCategoriaAsync(request.CategoriaId, ct);

        // O endereço é gravado pela aplicação; chamado e histórico inicial (ABERTO) pela procedure do banco.
        var id = await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var endereco = await enderecoResolver.ResolverAsync(request.Cep, request.Numero, request.Referencia, ct);
            return await procedures.AbrirChamadoAsync(
                request.Titulo.Trim(), request.Descricao.Trim(), foto.Bytes,
                usuarioId, endereco.Id, request.CategoriaId, ct);
        }, ct);

        logger.LogInformation("Chamado {ChamadoId} aberto pelo usuário {UsuarioId}.", id, usuarioId);
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
        if (!ehGestor && (atual.Status ?? Status.Aberto) != Status.Aberto)
            throw new DomainException("O chamado só pode ser editado pelo autor enquanto estiver 'ABERTO'.");

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

        var statusAtual = atual.Status ?? Status.Aberto;
        if (statusAtual == novo)
            throw new ConflitoException($"O chamado já está com o status '{novo}'.");

        var observacao = string.IsNullOrWhiteSpace(request.Observacao)
            ? $"Status alterado de {statusAtual} para {novo}."
            : request.Observacao.Trim();

        var status = await ObterStatusAsync(novo, ct);
        await procedures.AtualizarStatusAsync(id, status.Id, gestorId, observacao, ct); // valida o gestor no banco

        logger.LogInformation("Chamado {ChamadoId} movido de {De} para {Para} pelo gestor {GestorId}.",
            id, statusAtual, novo, gestorId);
        return await ObterAsync(id, gestorId, ct);
    }

    public async Task<ChamadoResponse?> ApoiarAsync(int id, int usuarioId, CancellationToken ct = default)
    {
        if (!await chamados.ExistsAsync(c => c.Id == id, ct))
            return null;

        // Repetir o apoio não duplica nem dá erro. Autor e chamados encerrados são barrados pelo banco.
        if (!await votos.ExistsAsync(v => v.ChamadoId == id && v.UsuarioId == usuarioId, ct))
        {
            await procedures.RegistrarVotoAsync(id, usuarioId, ct);
            logger.LogInformation("Usuário {UsuarioId} apoiou o chamado {ChamadoId}.", usuarioId, id);
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
            if ((atual.Status ?? Status.Aberto) != Status.Aberto)
                throw new DomainException("O chamado só pode ser excluído pelo autor enquanto estiver 'ABERTO'.");
        }

        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await chamados.ExcluirComDependenciasAsync(id, ct);
            return true;
        }, ct);

        logger.LogInformation("Chamado {ChamadoId} excluído por {UsuarioId}.", id, usuarioId);
        return true;
    }

    private async Task GarantirCategoriaAsync(int categoriaId, CancellationToken ct)
    {
        if (!await categorias.ExistsAsync(c => c.Id == categoriaId, ct))
            throw new DomainException("Categoria não encontrada.");
    }

    private async Task<Status> ObterStatusAsync(string nome, CancellationToken ct) =>
        await statusRepository.FirstOrDefaultAsync(s => s.Nome == nome, ct)
        ?? await statusRepository.AddAsync(new Status(nome), ct);
}