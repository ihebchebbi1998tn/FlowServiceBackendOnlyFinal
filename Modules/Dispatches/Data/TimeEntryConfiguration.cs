using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApi.Modules.Dispatches.Models;

namespace MyApi.Modules.Dispatches.Data
{
    public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
    {
        public void Configure(EntityTypeBuilder<TimeEntry> builder)
        {
            builder.ToTable("dispatch_time_entries");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(50);
            builder.Property(t => t.WorkType).HasMaxLength(50);
            builder.Property(t => t.Description).HasMaxLength(2000);
            builder.Property(t => t.Status).HasMaxLength(50);
            builder.Property(t => t.CreatedAt).IsRequired();
            builder.Property(t => t.HourlyRate).HasColumnType("decimal(10,2)");
            builder.Property(t => t.TotalCost).HasColumnType("decimal(10,2)");
        }
    }
}
