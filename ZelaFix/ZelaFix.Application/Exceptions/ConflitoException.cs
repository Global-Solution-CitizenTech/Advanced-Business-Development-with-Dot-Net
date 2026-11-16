namespace ZelaFix.Application.Exceptions;

/// <summary>Operação conflita com o estado atual dos dados (ex.: nome duplicado). Mapeada para HTTP 409.</summary>
public sealed class ConflitoException(string message) : Exception(message);