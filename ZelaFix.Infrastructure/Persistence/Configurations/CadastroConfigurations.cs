using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZelaFix.Domain.Entities;

namespace ZelaFix.Infrastructure.Persistence.Configurations;

public sealed class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("CATEGORIA");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ComoNumber5("ID_CATEGORIA").ValueGeneratedOnAdd();
        builder.Property(c => c.Nome).HasColumnName("NM_CATEGORIA").HasMaxLength(50).IsRequired();
    }
}

public sealed class StatusConfiguration : IEntityTypeConfiguration<Status>
{
    public void Configure(EntityTypeBuilder<Status> builder)
    {
        builder.ToTable("STATUS");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ComoNumber5("ID_STATUS").ValueGeneratedOnAdd();
        builder.Property(s => s.Nome).HasColumnName("NM_STATUS").HasMaxLength(20).IsRequired();
    }
}