namespace ZelaFix.Domain.Exceptions;

/// <summary>Violação de regra de negócio do domínio (mapeada para HTTP 400 no handler global).</summary>
public class DomainException(string message) : Exception(message);