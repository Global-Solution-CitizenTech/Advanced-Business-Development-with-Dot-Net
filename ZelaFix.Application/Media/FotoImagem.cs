using System.Text;
using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Application.Media;

public sealed record FotoDecodificada(byte[] Bytes, string ContentType);

/// <summary>Decodifica e valida a foto enviada em Base64 (a tabela só guarda o binário, sem tipo MIME).</summary>
public static class FotoImagem
{
    public const int TamanhoMaximoBytes = 5 * 1024 * 1024;

    public static FotoDecodificada Decodificar(string? base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
            throw new DomainException("A foto do chamado é obrigatória.");

        var conteudo = base64.Trim();

        // Aceita também o formato "data:image/jpeg;base64,...."
        if (conteudo.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var virgula = conteudo.IndexOf(',');
            if (virgula < 0)
                throw new DomainException("A foto deve estar codificada em Base64.");
            conteudo = conteudo[(virgula + 1)..];
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(conteudo);
        }
        catch (FormatException)
        {
            throw new DomainException("A foto deve estar codificada em Base64.");
        }

        if (bytes.Length == 0)
            throw new DomainException("A foto do chamado é obrigatória.");
        if (bytes.Length > TamanhoMaximoBytes)
            throw new DomainException("A foto deve ter no máximo 5 MB.");

        var tipo = DetectarContentType(bytes)
            ?? throw new DomainException("Formato de imagem não suportado. Use JPEG, PNG ou WebP.");

        return new FotoDecodificada(bytes, tipo);
    }

    /// <summary>Identifica o formato pelos primeiros bytes (assinatura do arquivo).</summary>
    public static string? DetectarContentType(byte[] b)
    {
        if (b.Length > 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
            return "image/jpeg";

        if (b.Length > 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47
            && b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A)
            return "image/png";

        if (b.Length > 12
            && Encoding.ASCII.GetString(b, 0, 4) == "RIFF"
            && Encoding.ASCII.GetString(b, 8, 4) == "WEBP")
            return "image/webp";

        return null;
    }
}