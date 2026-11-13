using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZelaFix.Domain.Entities;
using ZelaFix.Domain.Enums;

namespace ZelaFix.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("USUARIO");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ComoNumber5("ID_USUARIO").ValueGeneratedOnAdd();
        builder.Property(u => u.Nome).HasColumnName("NM_USUARIO").HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasColumnName("EMAIL").HasMaxLength(100).IsRequired();
        builder.Property(u => u.SenhaHash).HasColumnName("SENHA").HasMaxLength(60).IsRequired();

        builder.Property(u => u.Cargo)
            .HasColumnName("CARGO")
            .HasMaxLength(10)
            .IsRequired()
            .HasConversion(
                v => v.ToString().ToUpperInvariant(),
                v => Enum.Parse<Cargo>(v, true));

        builder.Property(u => u.EnderecoId).ComoNumber5("ENDERECO_ID_ENDERECO");

        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasOne(u => u.Endereco)
            .WithMany()
            .HasForeignKey(u => u.EnderecoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class TelefoneConfiguration : IEntityTypeConfiguration<Telefone>
{
    public void Configure(EntityTypeBuilder<Telefone> builder)
    {
        builder.ToTable("TELEFONE");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ComoNumber5("ID_TELEFONE").ValueGeneratedOnAdd();
        builder.Property(t => t.Ddd).HasColumnName("NUM_DDD").HasMaxLength(2).IsRequired();
        builder.Property(t => t.Numero).HasColumnName("NUM_TEL").HasMaxLength(9).IsRequired();
        builder.Property(t => t.UsuarioId).ComoNumber5("USUARIO_ID_USUARIO");

        builder.HasIndex(t => t.UsuarioId).IsUnique();

        builder.HasOne(t => t.Usuario)
            .WithOne(u => u.Telefone)
            .HasForeignKey<Telefone>(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}