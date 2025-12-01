namespace MyApi.Modules.Installations.DTOs
{
    public class LocationDto
    {
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? ZipCode { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }

    public class SpecificationsDto
    {
        public string? Processor { get; set; }
        public string? Ram { get; set; }
        public string? Storage { get; set; }
        public string? OperatingSystem { get; set; }
        public string? OsVersion { get; set; }
    }

    public class WarrantyDto
    {
        public bool HasWarranty { get; set; }
        public DateTime? WarrantyFrom { get; set; }
        public DateTime? WarrantyTo { get; set; }
        public string? WarrantyProvider { get; set; }
        public string? WarrantyType { get; set; }
    }

    public class MaintenanceDto
    {
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public string? MaintenanceFrequency { get; set; }
        public string? MaintenanceNotes { get; set; }
    }

    public class ContactInfoDto
    {
        public string? PrimaryContactName { get; set; }
        public string? PrimaryContactPhone { get; set; }
        public string? PrimaryContactEmail { get; set; }
        public string? SecondaryContactName { get; set; }
        public string? SecondaryContactPhone { get; set; }
        public string? SecondaryContactEmail { get; set; }
    }

    public class MetadataDto
    {
        public string[]? Tags { get; set; }
        public Dictionary<string, object>? CustomFields { get; set; }
    }

    public class InstallationDto
    {
        public string? Id { get; set; }
        public string? InstallationNumber { get; set; }
        public int ContactId { get; set; }
        public string? Name { get; set; }
        public string? Model { get; set; }
        public string? Manufacturer { get; set; }
        public string? SerialNumber { get; set; }
        public string? AssetTag { get; set; }
        public string? Category { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public LocationDto? Location { get; set; }
        public SpecificationsDto? Specifications { get; set; }
        public WarrantyDto? Warranty { get; set; }
        public MaintenanceDto? Maintenance { get; set; }
        public ContactInfoDto? Contact { get; set; }
        public MetadataDto? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class CreateInstallationDto
    {
        public int ContactId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }
        public string? AssetTag { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string Status { get; set; } = "active";
        public LocationDto? Location { get; set; }
        public SpecificationsDto? Specifications { get; set; }
        public WarrantyDto? Warranty { get; set; }
        public MaintenanceDto? Maintenance { get; set; }
        public ContactInfoDto? Contact { get; set; }
        public MetadataDto? Metadata { get; set; }
    }

    public class UpdateInstallationDto
    {
        public string? Name { get; set; }
        public string? Model { get; set; }
        public string? Manufacturer { get; set; }
        public string? SerialNumber { get; set; }
        public string? AssetTag { get; set; }
        public string? Category { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public LocationDto? Location { get; set; }
        public SpecificationsDto? Specifications { get; set; }
        public WarrantyDto? Warranty { get; set; }
        public MaintenanceDto? Maintenance { get; set; }
        public ContactInfoDto? Contact { get; set; }
        public MetadataDto? Metadata { get; set; }
    }

    public class MaintenanceHistoryDto
    {
        public string? Id { get; set; }
        public string? InstallationId { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public string? MaintenanceType { get; set; }
        public string? Description { get; set; }
        public string? Technician { get; set; }
        public int? Duration { get; set; }
        public string? Notes { get; set; }
        public DateTime? NextScheduledDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class PaginatedInstallationResponse
    {
        public List<InstallationDto> Installations { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }

    public class PaginationInfo
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
