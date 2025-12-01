using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApi.Modules.Installations.Models;

namespace MyApi.Modules.Installations.Data
{
    public class InstallationConfiguration : IEntityTypeConfiguration<Installation>
    {
        public void Configure(EntityTypeBuilder<Installation> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Id)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(i => i.InstallationNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(i => i.Name)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(i => i.Model)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(i => i.Manufacturer)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(i => i.SerialNumber)
                .HasMaxLength(255);

            builder.Property(i => i.AssetTag)
                .HasMaxLength(255);

            builder.Property(i => i.Category)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(i => i.Status)
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue("active");

            // Location properties
            builder.Property(i => i.LocationAddress)
                .HasMaxLength(500);

            builder.Property(i => i.LocationCity)
                .HasMaxLength(100);

            builder.Property(i => i.LocationState)
                .HasMaxLength(100);

            builder.Property(i => i.LocationCountry)
                .HasMaxLength(100);

            builder.Property(i => i.LocationZipCode)
                .HasMaxLength(20);

            builder.Property(i => i.LocationLatitude)
                .HasPrecision(10, 8);

            builder.Property(i => i.LocationLongitude)
                .HasPrecision(11, 8);

            // Specifications properties
            builder.Property(i => i.SpecificationsProcessor)
                .HasMaxLength(255);

            builder.Property(i => i.SpecificationsRam)
                .HasMaxLength(255);

            builder.Property(i => i.SpecificationsStorage)
                .HasMaxLength(255);

            builder.Property(i => i.SpecificationsOperatingSystem)
                .HasMaxLength(255);

            builder.Property(i => i.SpecificationsOsVersion)
                .HasMaxLength(255);

            // Warranty properties
            builder.Property(i => i.WarrantyProvider)
                .HasMaxLength(255);

            builder.Property(i => i.WarrantyType)
                .HasMaxLength(255);

            // Maintenance properties
            builder.Property(i => i.MaintenanceFrequency)
                .HasMaxLength(50);

            builder.Property(i => i.MaintenanceNotes)
                .HasMaxLength(1000);

            // Contact properties
            builder.Property(i => i.ContactPrimaryName)
                .HasMaxLength(255);

            builder.Property(i => i.ContactPrimaryPhone)
                .HasMaxLength(20);

            builder.Property(i => i.ContactPrimaryEmail)
                .HasMaxLength(255);

            builder.Property(i => i.ContactSecondaryName)
                .HasMaxLength(255);

            builder.Property(i => i.ContactSecondaryPhone)
                .HasMaxLength(20);

            builder.Property(i => i.ContactSecondaryEmail)
                .HasMaxLength(255);

            // Audit properties
            builder.Property(i => i.CreatedBy)
                .HasMaxLength(50);

            builder.Property(i => i.UpdatedBy)
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(i => i.InstallationNumber)
                .IsUnique();

            builder.HasIndex(i => i.SerialNumber)
                .IsUnique();

            builder.HasIndex(i => i.AssetTag)
                .IsUnique();

            builder.HasIndex(i => i.ContactId);
            builder.HasIndex(i => i.Status);
            builder.HasIndex(i => i.Category);
            builder.HasIndex(i => i.CreatedAt);

            // Relationships
            builder.HasMany(i => i.MaintenanceHistories)
                .WithOne(m => m.Installation)
                .HasForeignKey(m => m.InstallationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class MaintenanceHistoryConfiguration : IEntityTypeConfiguration<MaintenanceHistory>
    {
        public void Configure(EntityTypeBuilder<MaintenanceHistory> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(m => m.InstallationId)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(m => m.MaintenanceType)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(m => m.Description)
                .HasMaxLength(1000);

            builder.Property(m => m.Technician)
                .HasMaxLength(255);

            builder.Property(m => m.Notes)
                .HasMaxLength(2000);

            builder.Property(m => m.CreatedBy)
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(m => m.InstallationId);
            builder.HasIndex(m => m.MaintenanceDate);
            builder.HasIndex(m => m.MaintenanceType);
        }
    }
}
