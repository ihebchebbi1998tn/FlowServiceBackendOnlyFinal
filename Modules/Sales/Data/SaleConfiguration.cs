using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApi.Modules.Sales.Models;
using MyApi.Modules.Shared.Data.Configurations;

namespace MyApi.Modules.Sales.Data
{
    public class SaleConfiguration : IEntityConfiguration
    {
        public void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.ToTable("sales");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ContactId)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Stage)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Currency)
                    .IsRequired()
                    .HasMaxLength(3);

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(15,2)")
                    .IsRequired();

                entity.Property(e => e.Taxes)
                    .HasColumnType("decimal(15,2)");

                entity.Property(e => e.Discount)
                    .HasColumnType("decimal(15,2)");

                entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(15,2)")
                    .HasComputedColumnSql("(amount + COALESCE(taxes, 0) - COALESCE(discount, 0))", stored: true);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.HasIndex(e => e.ContactId)
                    .HasDatabaseName("idx_sales_contact_id");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("idx_sales_status");

                entity.HasIndex(e => e.Stage)
                    .HasDatabaseName("idx_sales_stage");

                entity.HasIndex(e => e.CreatedAt)
                    .HasDatabaseName("idx_sales_created_at");

                entity.HasIndex(e => e.UpdatedAt)
                    .HasDatabaseName("idx_sales_updated_at");

                entity.HasIndex(e => e.OfferId)
                    .HasDatabaseName("idx_sales_offer_id");

                entity.HasIndex(e => e.AssignedTo)
                    .HasDatabaseName("idx_sales_assigned_to");

                entity.HasMany(e => e.Items)
                    .WithOne(i => i.Sale)
                    .HasForeignKey(i => i.SaleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Activities)
                    .WithOne(a => a.Sale)
                    .HasForeignKey(a => a.SaleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }

    public class SaleItemConfiguration : IEntityConfiguration
    {
        public void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SaleItem>(entity =>
            {
                entity.ToTable("sale_items");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.ItemName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Quantity)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(15,2)")
                    .IsRequired();

                entity.Property(e => e.Discount)
                    .HasColumnType("decimal(15,2)");

                entity.Property(e => e.TotalPrice)
                    .HasColumnType("decimal(15,2)")
                    .HasComputedColumnSql(@"
                        (quantity * unit_price - COALESCE(
                            CASE 
                                WHEN discount_type = 'percentage' THEN (quantity * unit_price * discount / 100)
                                WHEN discount_type = 'fixed' THEN discount
                                ELSE 0
                            END, 0
                        ))
                    ", stored: true);

                entity.HasIndex(e => e.SaleId)
                    .HasDatabaseName("idx_sale_items_sale_id");

                entity.HasIndex(e => e.Type)
                    .HasDatabaseName("idx_sale_items_type");

                entity.HasIndex(e => e.ArticleId)
                    .HasDatabaseName("idx_sale_items_article_id");
            });
        }
    }

    public class SaleActivityConfiguration : IEntityConfiguration
    {
        public void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SaleActivity>(entity =>
            {
                entity.ToTable("sale_activities");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Description)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.CreatedByName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasIndex(e => e.SaleId)
                    .HasDatabaseName("idx_sale_activities_sale_id");

                entity.HasIndex(e => e.Type)
                    .HasDatabaseName("idx_sale_activities_type");

                entity.HasIndex(e => e.CreatedAt)
                    .HasDatabaseName("idx_sale_activities_created_at");
            });
        }
    }
}
