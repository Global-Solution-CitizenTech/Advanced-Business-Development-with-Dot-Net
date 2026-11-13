namespace ZelaFix.Application.Exceptions;

/// <summary>O usuário está autenticado, mas não pode executar esta ação. Mapeada para HTTP 403.</summary>
public sealed class AcessoNegadoException(string message) : Exception(message);