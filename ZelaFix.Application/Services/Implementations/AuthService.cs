using Microsoft.Extensions.Logging;
using ZelaFix.Application.DTOs;
using ZelaFix.Application.Exceptions;
using ZelaFix.Application.Repositories;
using ZelaFix.Application.Services.Interfaces;
using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Enums;

namespace ZelaFix.Application.Services.Implementations;

public sealed class AuthService(
    IUsuarioRepository usuarios,
    IRepository<Telefone> telefones,
    IEnderecoResolver enderecoResolver,
    ITokenService tokenService,
    IUnitOfWork unitOfWork,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await usuarios.ExistsAsync(u => u.Email == email, ct))
            throw new ConflitoException("Já existe um usuário com este e-mail.");

        var (ddd, numeroTelefone) = request.SepararTelefone();

        // Endereço, usuário e telefone são gravados juntos: se algo falhar, nada fica pela metade.
        var usuarioId = await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var endereco = await enderecoResolver.ResolverAsync(request.Cep, request.Numero, request.Referencia, ct);
            var usuario = await usuarios.AddAsync(
                new Usuario(request.Nome, email, request.Senha, Cargo.Cidadao, endereco.Id), ct);
            await telefones.AddAsync(new Telefone(ddd, numeroTelefone, usuario.Id), ct);
            return usuario.Id;
        }, ct);

        var cadastrado = await usuarios.GetComDetalhesAsync(usuarioId, ct)
            ?? throw new InvalidOperationException("Usuário recém-cadastrado não foi encontrado.");

        logger.LogInformation("Usuário {UsuarioId} cadastrado como {Cargo}.", usuarioId, Cargo.Cidadao);
        return Responder(cadastrado);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var usuario = await usuarios.GetByEmailAsync(email, ct);

        if (usuario is null || !usuario.VerificarSenha(request.Senha))
        {
            logger.LogWarning("Login negado para {Email}.", email);
            return null;
        }

        logger.LogInformation("Usuário {UsuarioId} autenticado.", usuario.Id);
        return Responder(usuario);
    }

    public async Task<UsuarioResponse?> GetPerfilAsync(int usuarioId, CancellationToken ct = default) =>
        await usuarios.GetComDetalhesAsync(usuarioId, ct) is { } usuario
            ? UsuarioResponse.FromDomain(usuario)
            : null;

    private AuthResponse Responder(Usuario usuario)
    {
        var token = tokenService.GerarToken(usuario);
        return new AuthResponse(token.Token, token.ExpiraEmUtc, UsuarioResponse.FromDomain(usuario));
    }
}