using ZelaFix.Domain.Entities;

namespace ZelaFix.Application.Services.Interfaces;

public sealed record TokenEmitido(string Token, DateTime ExpiraEmUtc);

public interface ITokenService
{
    TokenEmitido GerarToken(Usuario usuario);
}