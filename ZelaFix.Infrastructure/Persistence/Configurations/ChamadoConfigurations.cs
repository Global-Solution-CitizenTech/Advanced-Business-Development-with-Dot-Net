using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZelaFix.Domain.Entities;

namespace ZelaFix.Infrastructure.Persistence.Configurations;

public sealed class ChamadoConfiguration : IEntityTypeConfiguration<Chamado>
{
    public void Configure(EntityTypeBuilder<Chamado> builder)
    {
        builder.ToTable("CHAMADO");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ComoNumber5("ID_CHAMADO").ValueGeneratedOnAdd();
        builder.Property(c => c.Titulo).HasColumnName("TITULO").HasMaxLength(100).IsRequired();
        builder.Property(c => c.Descricao).HasColumnName("DESCRICAO").HasMaxLength(1000).IsRequired();
        builder.Property(c => c.Foto).HasColumnName("FOTO").HasColumnType("BLOB").IsRequired();
        builder.Property(c => c.DataAbertura).HasColumnName("DT_ABERTURA").HasColumnType("DATE");
        builder.Property(c => c.UsuarioId).ComoNumber5("USUARIO_ID_USUARIO");
        builder.Property(c => c.EnderecoId).ComoNumber5("ENDERECO_ID_ENDERECO");
        builder.Property(c => c.CategoriaId).ComoNumber5("CATEGORIA_ID_CATEGORIA");

        builder.HasOne(c => c.Usuario)
            .WithMany(u => u.Chamados)
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Endereco)
            .WithMany()
            .HasForeignKey(c => c.EnderecoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Categoria)
            .WithMany()
            .HasForeignKey(c => c.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class HistChamadoConfiguration : IEntityTypeConfiguration<HistChamado>
{
    public void Configure(EntityTypeBuilder<HistChamado> builder)
    {
        builder.ToTable("HIST_CHAMADO");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ComoNumber5("ID_HIST").ValueGeneratedOnAdd();
        builder.Property(h => h.DataMudanca).HasColumnName("DT_MUDANCA").HasColumnType("DATE");
        builder.Property(h => h.Observacoes).HasColumnName("OBSERVACOES").HasMaxLength(255).IsRequired();
        builder.Property(h => h.ChamadoId).ComoNumber5("CHAMADO_ID_CHAMADO");
        builder.Property(h => h.UsuarioId).ComoNumber5("USUARIO_ID_USUARIO");
        builder.Property(h => h.StatusId).ComoNumber5("STATUS_ID_STATUS");

        builder.HasOne(h => h.Chamado)
            .WithMany(c => c.Historico)
            .HasForeignKey(h => h.ChamadoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Usuario)
            .WithMany()
            .HasForeignKey(h => h.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Status)
            .WithMany()
            .HasForeignKey(h => h.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class VotoChamadoConfiguration : IEntityTypeConfiguration<VotoChamado>
{
    public void Configure(EntityTypeBuilder<VotoChamado> builder)
    {
        builder.ToTable("VOTO_CHAMADO");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ComoNumber5("ID_VOTO").ValueGeneratedOnAdd();
        builder.Property(v => v.DataVoto).HasColumnName("DT_VOTO").HasColumnType("DATE");
        builder.Property(v => v.ChamadoId).ComoNumber5("CHAMADO_ID_CHAMADO");
        builder.Property(v => v.UsuarioId).ComoNumber5("USUARIO_ID_USUARIO");

        builder.HasOne(v => v.Chamado)
            .WithMany(c => c.Votos)
            .HasForeignKey(v => v.ChamadoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Usuario)
            .WithMany()
            .HasForeignKey(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}