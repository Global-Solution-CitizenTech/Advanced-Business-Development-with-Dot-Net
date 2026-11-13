using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ZelaFix.Application.Auth;
using ZelaFix.Application.Services.Interfaces;
using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Enums;

namespace ZelaFix.Application.Services.Implementations;

/// <summary>Emite tokens JWT (RFC 7519) assinados com HMAC-SHA256.</summary>
public sealed class TokenService(JwtSettings settings) : ITokenService
{
    public TokenEmitido GerarToken(Usuario usuario)
    {
        var expira = DateTime.UtcNow.AddMinutes(settings.ExpirationMinutes);

        var claims = new[]
        {
            new Claim(ClaimsNomes.Subject, usuario.Id.ToString()),
            new Claim(ClaimsNomes.Email, usuario.Email),
            new Claim(ClaimsNomes.Nome, usuario.Nome),
            new Claim(ClaimsNomes.Perfil, usuario.Cargo.ParaPerfil())
        };

        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expira,
            signingCredentials: credenciais);

        return new TokenEmitido(new JwtSecurityTokenHandler().WriteToken(jwt), expira);
    }
}