using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZelaFix.Application.Repositories;
using ZelaFix.Infrastructure.Persistence;
using ZelaFix.Infrastructure.Persistence.Repositories;

namespace ZelaFix.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ZelaFixDbContext>(options =>
            options.UseOracle(connectionString, o => o.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IChamadoRepository, ChamadoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}