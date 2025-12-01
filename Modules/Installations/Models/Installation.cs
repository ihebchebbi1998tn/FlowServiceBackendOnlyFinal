using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Modules.Installations.Models
{
    [Table("installations")]
    public class Installation
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = string.Empty;

        [Column("installation_number")]
        public string InstallationNumber { get; set; } = string.Empty;

        [Column("contact_id")]
        public int ContactId { get; set; }

        [Column("name")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Column("model")]
        [MaxLength(255)]
        public string Model { get; set; } = string.Empty;

        [Column("manufacturer")]
        [MaxLength(255)]
        public string Manufacturer { get; set; } = string.Empty;

        [Column("serial_number")]
        [MaxLength(255)]
        public string? SerialNumber { get; set; }

        [Column("asset_tag")]
        [MaxLength(255)]
        public string? AssetTag { get; set; }

        [Column("category")]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Column("type")]
        [MaxLength(50)]
        public string? Type { get; set; }

        [Column("status")]
        [MaxLength(50)]
        public string Status { get; set; } = "active";

        // Location fields
        [Column("location_address")]
        [MaxLength(500)]
        public string? LocationAddress { get; set; }

        [Column("location_city")]
        [MaxLength(100)]
        public string? LocationCity { get; set; }

        [Column("location_state")]
        [MaxLength(100)]
        public string? LocationState { get; set; }

        [Column("location_country")]
        [MaxLength(100)]
        public string? LocationCountry { get; set; }

        [Column("location_zip_code")]
        [MaxLength(20)]
        public string? LocationZipCode { get; set; }

        [Column("location_latitude")]
        public decimal? LocationLatitude { get; set; }

        [Column("location_longitude")]
        public decimal? LocationLongitude { get; set; }

        // Specifications fields
        [Column("specifications_processor")]
        [MaxLength(255)]
        public string? SpecificationsProcessor { get; set; }

        [Column("specifications_ram")]
        [MaxLength(255)]
        public string? SpecificationsRam { get; set; }

        [Column("specifications_storage")]
        [MaxLength(255)]
        public string? SpecificationsStorage { get; set; }

        [Column("specifications_operating_system")]
        [MaxLength(255)]
        public string? SpecificationsOperatingSystem { get; set; }

        [Column("specifications_os_version")]
        [MaxLength(255)]
        public string? SpecificationsOsVersion { get; set; }

        // Warranty fields
        [Column("warranty_has_warranty")]
        public bool WarrantyHasWarranty { get; set; }

        [Column("warranty_from")]
        public DateTime? WarrantyFrom { get; set; }

        [Column("warranty_to")]
        public DateTime? WarrantyTo { get; set; }

        [Column("warranty_provider")]
        [MaxLength(255)]
        public string? WarrantyProvider { get; set; }

        [Column("warranty_type")]
        [MaxLength(255)]
        public string? WarrantyType { get; set; }

        // Maintenance fields
        [Column("maintenance_last_date")]
        public DateTime? MaintenanceLastDate { get; set; }

        [Column("maintenance_next_date")]
        public DateTime? MaintenanceNextDate { get; set; }

        [Column("maintenance_frequency")]
        [MaxLength(50)]
        public string? MaintenanceFrequency { get; set; }

        [Column("maintenance_notes")]
        [MaxLength(1000)]
        public string? MaintenanceNotes { get; set; }

        // Contact fields
        [Column("contact_primary_name")]
        [MaxLength(255)]
        public string? ContactPrimaryName { get; set; }

        [Column("contact_primary_phone")]
        [MaxLength(20)]
        public string? ContactPrimaryPhone { get; set; }

        [Column("contact_primary_email")]
        [MaxLength(255)]
        public string? ContactPrimaryEmail { get; set; }

        [Column("contact_secondary_name")]
        [MaxLength(255)]
        public string? ContactSecondaryName { get; set; }

        [Column("contact_secondary_phone")]
        [MaxLength(20)]
        public string? ContactSecondaryPhone { get; set; }

        [Column("contact_secondary_email")]
        [MaxLength(255)]
        public string? ContactSecondaryEmail { get; set; }

        // Metadata fields
        [Column("tags")]
        public string[]? Tags { get; set; }

        [Column("custom_fields")]
        public string? CustomFields { get; set; } // JSON

        // Audit fields
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("created_by")]
        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        [Column("updated_by")]
        [MaxLength(50)]
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<MaintenanceHistory> MaintenanceHistories { get; set; } = new List<MaintenanceHistory>();
    }
}
