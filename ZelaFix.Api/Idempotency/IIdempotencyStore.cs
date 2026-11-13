namespace ZelaFix.Api.Idempotency;

public enum EstadoIdempotencia
{
    Novo,
    EmAndamento,
    Concluido,
    PayloadDiferente
}

public sealed record RespostaArmazenada(int StatusCode, string Corpo);

public sealed record ResultadoIdempotencia(EstadoIdempotencia Estado, RespostaArmazenada? Resposta = null);

/// <summary>Guarda o resultado de requisições já processadas. A interface permite trocar a memória por Oracle ou Redis.</summary>
public interface IIdempotencyStore
{
    Task<ResultadoIdempotencia> IniciarAsync(string chave, string impressao, CancellationToken ct = default);

    Task ConcluirAsync(string chave, RespostaArmazenada resposta, CancellationToken ct = default);

    Task LiberarAsync(string chave, CancellationToken ct = default);
}