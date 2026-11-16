using System.Collections.Concurrent;

namespace ZelaFix.Api.Idempotency;

/// <summary>Implementação em memória (uma instância da API). Os registros expiram em 24 horas.</summary>
public sealed class MemoryIdempotencyStore : IIdempotencyStore
{
    private static readonly TimeSpan Validade = TimeSpan.FromHours(24);
    private const int LimiteParaLimpeza = 1000;

    private readonly ConcurrentDictionary<string, Registro> _registros = new();

    public Task<ResultadoIdempotencia> IniciarAsync(string chave, string impressao, CancellationToken ct = default)
    {
        LimparExpirados();

        while (true)
        {
            var agora = DateTimeOffset.UtcNow;

            if (_registros.TryAdd(chave, new Registro(impressao, agora.Add(Validade))))
                return Task.FromResult(new ResultadoIdempotencia(EstadoIdempotencia.Novo));

            if (!_registros.TryGetValue(chave, out var existente))
                continue; // foi removido entre as duas chamadas: tenta de novo

            if (existente.ExpiraEm <= agora)
            {
                _registros.TryRemove(new KeyValuePair<string, Registro>(chave, existente));
                continue;
            }

            if (!string.Equals(existente.Impressao, impressao, StringComparison.Ordinal))
                return Task.FromResult(new ResultadoIdempotencia(EstadoIdempotencia.PayloadDiferente));

            var resposta = existente.Resposta;
            return Task.FromResult(resposta is null
                ? new ResultadoIdempotencia(EstadoIdempotencia.EmAndamento)
                : new ResultadoIdempotencia(EstadoIdempotencia.Concluido, resposta));
        }
    }

    public Task ConcluirAsync(string chave, RespostaArmazenada resposta, CancellationToken ct = default)
    {
        if (_registros.TryGetValue(chave, out var registro))
            registro.Resposta = resposta;

        return Task.CompletedTask;
    }

    public Task LiberarAsync(string chave, CancellationToken ct = default)
    {
        _registros.TryRemove(chave, out _);
        return Task.CompletedTask;
    }

    private void LimparExpirados()
    {
        if (_registros.Count < LimiteParaLimpeza)
            return;

        var agora = DateTimeOffset.UtcNow;
        foreach (var par in _registros.Where(p => p.Value.ExpiraEm <= agora))
            _registros.TryRemove(par);
    }

    private sealed class Registro(string impressao, DateTimeOffset expiraEm)
    {
        public string Impressao { get; } = impressao;
        public DateTimeOffset ExpiraEm { get; } = expiraEm;
        public volatile RespostaArmazenada? Resposta;
    }
}