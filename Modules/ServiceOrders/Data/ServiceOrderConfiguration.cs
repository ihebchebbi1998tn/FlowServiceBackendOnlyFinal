using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApi.Modules.ServiceOrders.Models;

namespace MyApi.Modules.ServiceOrders.Data
{
    public class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
    {
        public void Configure(EntityTypeBuilder<ServiceOrder> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.Status)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.Priority)
                .HasMaxLength(20);

            builder.Property(s => s.PaymentStatus)
                .HasMaxLength(20);

            builder.Property(s => s.PaymentTerms)
                .HasMaxLength(20);

            // Indexes
            builder.HasIndex(s => s.OrderNumber).IsUnique();
            builder.HasIndex(s => s.SaleId);
            builder.HasIndex(s => s.OfferId);
            builder.HasIndex(s => s.ContactId);
            builder.HasIndex(s => s.Status);
            builder.HasIndex(s => s.Priority);
            builder.HasIndex(s => s.CreatedAt);
            builder.HasIndex(s => s.StartDate);

            // Navigation
            builder.HasMany(s => s.Jobs)
                .WithOne(j => j.ServiceOrder)
                .HasForeignKey(j => j.ServiceOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ServiceOrderJobConfiguration : IEntityTypeConfiguration<ServiceOrderJob>
    {
        public void Configure(EntityTypeBuilder<ServiceOrderJob> builder)
        {
            builder.HasKey(j => j.Id);

            builder.Property(j => j.Title)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(j => j.Status)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(j => j.WorkType)
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(j => j.ServiceOrderId);
            builder.HasIndex(j => j.Status);
            builder.HasIndex(j => j.InstallationId);
        }
    }
}
