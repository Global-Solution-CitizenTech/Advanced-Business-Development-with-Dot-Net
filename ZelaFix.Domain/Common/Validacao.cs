using ZelaFix.Domain.Exceptions;

namespace ZelaFix.Domain.Common;

internal static class Validacao
{
    public static string Texto(string? valor, string campo, int maximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException($"{campo} não pode estar em branco.");

        valor = valor.Trim();
        if (valor.Length > maximo)
            throw new DomainException($"{campo} deve ter no máximo {maximo} caracteres.");

        return valor;
    }

    public static int Id(int valor, string mensagem) =>
        valor > 0 ? valor : throw new DomainException(mensagem);
}