using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZelaFix.Domain.Entities;

namespace ZelaFix.Infrastructure.Persistence.Configurations;

public sealed class EstadoConfiguration : IEntityTypeConfiguration<Estado>
{
    public void Configure(EntityTypeBuilder<Estado> builder)
    {
        builder.ToTable("ESTADO");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ComoNumber5("ID_ESTADO").ValueGeneratedOnAdd();
        builder.Property(e => e.Nome).HasColumnName("NM_ESTADO").HasMaxLength(30).IsRequired();
    }
}

public sealed class CidadeConfiguration : IEntityTypeConfiguration<Cidade>
{
    public void Configure(EntityTypeBuilder<Cidade> builder)
    {
        builder.ToTable("CIDADE");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ComoNumber5("ID_CIDADE").ValueGeneratedOnAdd();
        builder.Property(c => c.Nome).HasColumnName("NM_CIDADE").HasMaxLength(50).IsRequired();
        builder.Property(c => c.EstadoId).ComoNumber5("ESTADO_ID_ESTADO");

        builder.HasOne(c => c.Estado)
            .WithMany(e => e.Cidades)
            .HasForeignKey(c => c.EstadoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class BairroConfiguration : IEntityTypeConfiguration<Bairro>
{
    public void Configure(EntityTypeBuilder<Bairro> builder)
    {
        builder.ToTable("BAIRRO");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ComoNumber5("ID_BAIRRO").ValueGeneratedOnAdd();
        builder.Property(b => b.Nome).HasColumnName("NM_BAIRRO").HasMaxLength(50).IsRequired();
        builder.Property(b => b.CidadeId).ComoNumber5("CIDADE_ID_CIDADE");

        builder.HasOne(b => b.Cidade)
            .WithMany(c => c.Bairros)
            .HasForeignKey(b => b.CidadeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class EnderecoConfiguration : IEntityTypeConfiguration<Endereco>
{
    public void Configure(EntityTypeBuilder<Endereco> builder)
    {
        builder.ToTable("ENDERECO");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ComoNumber5("ID_ENDERECO").ValueGeneratedOnAdd();
        builder.Property(e => e.Cep).HasColumnName("CEP").HasMaxLength(8).IsRequired();
        builder.Property(e => e.Rua).HasColumnName("RUA").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Numero).HasColumnName("NUMERO").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Referencia).HasColumnName("REFERENCIA").HasMaxLength(50).IsRequired();
        builder.Property(e => e.BairroId).ComoNumber5("BAIRRO_ID_BAIRRO");

        builder.HasOne(e => e.Bairro)
            .WithMany()
            .HasForeignKey(e => e.BairroId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}