using ZelaFix.Application.Services.Interfaces;

namespace ZelaFix.Tests.Integration.Fixtures;

public sealed class FakeViaCepService : IViaCepService
{
    public Task<ViaCepResponse?> ConsultarAsync(string cep, CancellationToken ct = default) =>
        Task.FromResult<ViaCepResponse?>(cep == "01001000"
            ? new ViaCepResponse("01001-000", "Praça da Sé", "Sé", "São Paulo", "SP", "São Paulo", null)
            : null);
}