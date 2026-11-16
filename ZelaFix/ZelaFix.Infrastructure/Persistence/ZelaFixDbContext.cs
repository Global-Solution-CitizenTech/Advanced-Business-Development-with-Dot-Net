using Microsoft.EntityFrameworkCore;
using ZelaFix.Domain.Entities;

namespace ZelaFix.Infrastructure.Persistence;

public class ZelaFixDbContext(DbContextOptions<ZelaFixDbContext> options) : DbContext(options)
{
    public DbSet<Estado> Estados => Set<Estado>();
    public DbSet<Cidade> Cidades => Set<Cidade>();
    public DbSet<Bairro> Bairros => Set<Bairro>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Status> Status => Set<Status>();
    public DbSet<Prioridade> Prioridades => Set<Prioridade>();
    public DbSet<OrgaoGoverno> OrgaosGoverno => Set<OrgaoGoverno>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Telefone> Telefones => Set<Telefone>();
    public DbSet<Chamado> Chamados => Set<Chamado>();
    public DbSet<HistChamado> Historicos => Set<HistChamado>();
    public DbSet<VotoChamado> Votos => Set<VotoChamado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ZelaFixDbContext).Assembly);
    }
}