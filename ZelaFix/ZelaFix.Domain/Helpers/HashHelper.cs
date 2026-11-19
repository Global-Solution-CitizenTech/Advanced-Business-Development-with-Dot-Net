namespace ZelaFix.Domain.Helpers;

/// <summary>
/// Hash de senha com BCrypt. O salt já faz parte do hash (60 caracteres),
/// o que cabe na coluna SENHA VARCHAR2(60) do banco.
/// </summary>
public static class HashHelper
{
    public static string Hash(string senhaPura) => BCrypt.Net.BCrypt.HashPassword(senhaPura);

    public static bool Verificar(string senhaPura, string hash)
    {
        if (string.IsNullOrWhiteSpace(senhaPura) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(senhaPura, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}