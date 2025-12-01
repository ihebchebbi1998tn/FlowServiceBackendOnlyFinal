using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApi.Modules.Dispatches.Models;

namespace MyApi.Modules.Dispatches.Data
{
    public class DispatchConfiguration : IEntityTypeConfiguration<Dispatch>
    {
        public void Configure(EntityTypeBuilder<Dispatch> builder)
        {
            builder.ToTable("dispatches");
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Id).HasMaxLength(50);
            builder.Property(d => d.DispatchNumber).IsRequired().HasMaxLength(100);
            builder.Property(d => d.Status).IsRequired().HasMaxLength(50);
            builder.Property(d => d.Priority).IsRequired().HasMaxLength(20);
            builder.Property(d => d.WorkLocationJson).HasColumnType("jsonb");
            builder.Property(d => d.CreatedAt).IsRequired();
            builder.Property(d => d.UpdatedAt).IsRequired();
            builder.Property(d => d.IsDeleted).HasDefaultValue(false);

            builder.HasMany(d => d.TimeEntries)
                .WithOne()
                .HasForeignKey(t => t.DispatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.Expenses)
                .WithOne()
                .HasForeignKey(e => e.DispatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.MaterialsUsed)
                .WithOne()
                .HasForeignKey(m => m.DispatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.Attachments)
                .WithOne()
                .HasForeignKey(a => a.DispatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.Notes)
                .WithOne()
                .HasForeignKey(n => n.DispatchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
