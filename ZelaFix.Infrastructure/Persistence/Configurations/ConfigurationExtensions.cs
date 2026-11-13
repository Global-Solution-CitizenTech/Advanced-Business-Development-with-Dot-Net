using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ZelaFix.Infrastructure.Persistence.Configurations;

internal static class ConfigurationExtensions
{
    public static PropertyBuilder<int> ComoNumber5(this PropertyBuilder<int> property, string coluna) =>
        property.HasColumnName(coluna).HasColumnType("NUMBER(5)");
}