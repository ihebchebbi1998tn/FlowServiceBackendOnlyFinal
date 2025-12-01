using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApi.Modules.Dispatches.Models;

namespace MyApi.Modules.Dispatches.Data
{
    public class DispatchTechnicianConfiguration : IEntityTypeConfiguration<DispatchTechnician>
    {
        public void Configure(EntityTypeBuilder<DispatchTechnician> builder)
        {
            builder.ToTable("dispatch_technicians");
            builder.HasKey(dt => new { dt.DispatchId, dt.TechnicianId });
            builder.Property(dt => dt.DispatchId).HasMaxLength(50);
            builder.Property(dt => dt.TechnicianId).HasMaxLength(50);
            builder.Property(dt => dt.Name).HasMaxLength(255);
            builder.Property(dt => dt.Email).HasMaxLength(255);
        }
    }
}
