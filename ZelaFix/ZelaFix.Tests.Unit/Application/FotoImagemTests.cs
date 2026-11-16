using ZelaFix.Application.Media;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Tests.Unit.Application;

public class FotoImagemTests
{
    public const string PngBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";

    [Fact]
    public void Decodificar_ComPngValido_DeveDetectarContentType()
    {
        // Arrange / Act
        var foto = FotoImagem.Decodificar(PngBase64);

        // Assert
        Assert.Equal("image/png", foto.ContentType);
        Assert.NotEmpty(foto.Bytes);
    }

    [Fact]
    public void Decodificar_ComPrefixoDataUri_DeveAceitar()
    {
        // Arrange / Act
        var foto = FotoImagem.Decodificar("data:image/png;base64," + PngBase64);

        // Assert
        Assert.Equal("image/png", foto.ContentType);
    }

    [Fact]
    public void Decodificar_ComBase64Invalido_DeveLancarDomainException()
    {
        // Arrange / Act
        var act = () => FotoImagem.Decodificar("isto-nao-e-base64!!!");

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Decodificar_ComFormatoNaoSuportado_DeveLancarDomainException()
    {
        // Arrange
        var texto = Convert.ToBase64String("um texto qualquer"u8.ToArray());

        // Act
        var act = () => FotoImagem.Decodificar(texto);

        // Assert
        Assert.Throws<DomainException>(act);
    }
}