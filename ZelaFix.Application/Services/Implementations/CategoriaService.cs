using Microsoft.Extensions.Logging;
using ZelaFix.Application.DTOs;
using ZelaFix.Application.Exceptions;
using ZelaFix.Application.Repositories;
using ZelaFix.Application.Services.Interfaces;
using ZelaFix.Domain.Entities;

namespace ZelaFix.Application.Services.Implementations;

public sealed class CategoriaService(
    IRepository<Categoria> repository,
    ILogger<CategoriaService> logger) : ICategoriaService
{
    public async Task<IReadOnlyList<CategoriaResponse>> GetAllAsync(CancellationToken ct = default) =>
        (await repository.GetAllAsync(ct)).Select(CategoriaResponse.FromDomain).ToList();

    public async Task<CategoriaResponse?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await repository.GetByIdAsync(id, ct) is { } categoria ? CategoriaResponse.FromDomain(categoria) : null;

    public async Task<CategoriaResponse> CreateAsync(CategoriaRequest request, CancellationToken ct = default)
    {
        var nome = request.Nome.Trim();
        if (await ExisteNomeAsync(nome, ignorarId: null, ct))
            throw new ConflitoException("Já existe uma categoria com este nome.");

        var categoria = await repository.AddAsync(new Categoria(nome), ct);
        logger.LogInformation("Categoria {CategoriaId} ('{Nome}') cadastrada.", categoria.Id, categoria.Nome);
        return CategoriaResponse.FromDomain(categoria);
    }

    public async Task<CategoriaResponse?> UpdateAsync(int id, CategoriaRequest request, CancellationToken ct = default)
    {
        var categoria = await repository.GetByIdAsync(id, ct);
        if (categoria is null)
            return null;

        var nome = request.Nome.Trim();
        if (await ExisteNomeAsync(nome, ignorarId: id, ct))
            throw new ConflitoException("Já existe uma categoria com este nome.");

        categoria.Atualizar(nome);
        await repository.UpdateAsync(categoria, ct);
        logger.LogInformation("Categoria {CategoriaId} atualizada para '{Nome}'.", id, categoria.Nome);
        return CategoriaResponse.FromDomain(categoria);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var removida = await repository.DeleteAsync(id, ct);
        if (removida)
            logger.LogInformation("Categoria {CategoriaId} removida.", id);

        return removida;
    }

    private Task<bool> ExisteNomeAsync(string nome, int? ignorarId, CancellationToken ct)
    {
        var normalizado = nome.ToLowerInvariant();
        return repository.ExistsAsync(
            c => c.Nome.ToLower() == normalizado && (ignorarId == null || c.Id != ignorarId), ct);
    }
}