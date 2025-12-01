using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApi.Modules.Dispatches.Models;

namespace MyApi.Modules.Dispatches.Data
{
    public class MaterialUsageConfiguration : IEntityTypeConfiguration<MaterialUsage>
    {
        public void Configure(EntityTypeBuilder<MaterialUsage> builder)
        {
            builder.ToTable("dispatch_materials");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).HasMaxLength(50);
            builder.Property(m => m.ArticleId).HasMaxLength(50);
            builder.Property(m => m.ArticleName).HasMaxLength(255);
            builder.Property(m => m.Sku).HasMaxLength(100);
            builder.Property(m => m.UnitPrice).HasColumnType("decimal(10,2)");
            builder.Property(m => m.TotalPrice).HasColumnType("decimal(10,2)");
            builder.Property(m => m.Status).HasMaxLength(50);
            builder.Property(m => m.CreatedAt).IsRequired();
        }
    }
}
