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
        builder.Property(c => c.Nome).HasColumnName("NM_CATEGORIA").HasMaxLength(50).IsUnicode(false).IsRequired();
        builder.HasIndex(c => c.Nome).IsUnique();
        builder.ToTable("CATEGORIA", table => table.HasCheckConstraint(
            "CK_CATEGORIA_NOME",
            "NM_CATEGORIA IN ('ILUMINACAO_PUBLICA', 'VIAS_E_PAVIMENTACAO', 'DRENAGEM_E_ESGOTO', 'LIMPEZA_URBANA', 'ARVORES_E_PARQUES', 'SINALIZACAO_E_TRANSITO')"));
    }
}

public sealed class StatusConfiguration : IEntityTypeConfiguration<Status>
{
    public void Configure(EntityTypeBuilder<Status> builder)
    {
        builder.ToTable("STATUS");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ComoNumber5("ID_STATUS").ValueGeneratedOnAdd();
        builder.Property(s => s.Nome).HasColumnName("NM_STATUS").HasMaxLength(20).IsUnicode(false).IsRequired();
        builder.ToTable("STATUS", table => table.HasCheckConstraint(
            "CK_STATUS_NOME",
            "NM_STATUS IN ('PENDENTE', 'EM_ANALISE', 'EM_EXECUCAO', 'CONCLUIDO', 'REJEITADO')"));
    }
}

public sealed class PrioridadeConfiguration : IEntityTypeConfiguration<Prioridade>
{
    public void Configure(EntityTypeBuilder<Prioridade> builder)
    {
        builder.ToTable("PRIORIDADE", table => table.HasCheckConstraint(
            "CK_PRIORIDADE_NOME",
            "NM_PRIORIDADE IN ('BAIXA', 'MEDIA', 'ALTA', 'URGENTE')"));
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ComoNumber5("ID_PRIORIDADE").ValueGeneratedOnAdd();
        builder.Property(p => p.Nome).HasColumnName("NM_PRIORIDADE").HasMaxLength(20).IsUnicode(false).IsRequired();
        builder.HasIndex(p => p.Nome).IsUnique();
    }
}

public sealed class OrgaoGovernoConfiguration : IEntityTypeConfiguration<OrgaoGoverno>
{
    public void Configure(EntityTypeBuilder<OrgaoGoverno> builder)
    {
        builder.ToTable("ORGAO_GOVERNO", table => table.HasCheckConstraint(
            "CK_ORGAO_GOVERNO_NOME",
            "ORGAO_GOVERNO IN ('SECRETARIA_OBRAS', 'CET_TRANSITO', 'ILUMINACAO_PUBLICA', 'DEFESA_CIVIL', 'SUBPREFEITURA', 'SANEAMENTO_BASICO')"));
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ComoNumber5("ID_ORGAO").ValueGeneratedOnAdd();
        builder.Property(o => o.Nome).HasColumnName("ORGAO_GOVERNO").HasMaxLength(100).IsUnicode(false).IsRequired();
        builder.HasIndex(o => o.Nome).IsUnique();
    }
}