using System.IdentityModel.Tokens.Jwt;
using ZelaFix.Application.Auth;
using ZelaFix.Application.Services.Implementations;
using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Enums;

namespace ZelaFix.Tests.Unit.Application;

public class TokenServiceTests
{
    [Fact]
    public void GerarToken_ParaGestor_DeveConterClaimDePerfilEExpiracao()
    {
        // Arrange
        var settings = new JwtSettings { Key = new string('k', 40), ExpirationMinutes = 30 };
        var gestor = new Usuario("Gestor", "gestor@teste.com", "senha123", Cargo.Gestor, 1);

        // Act
        var emitido = new TokenService(settings).GerarToken(gestor);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(emitido.Token);

        // Assert
        Assert.Equal("GESTOR", jwt.Claims.First(c => c.Type == ClaimsNomes.Perfil).Value);
        Assert.Equal("gestor@teste.com", jwt.Claims.First(c => c.Type == ClaimsNomes.Email).Value);
        Assert.True(emitido.ExpiraEmUtc > DateTime.UtcNow.AddMinutes(25));
    }
}