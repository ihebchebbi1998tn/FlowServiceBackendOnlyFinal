using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Modules.Installations.DTOs;
using MyApi.Modules.Installations.Models;
using System.Text.Json;

namespace MyApi.Modules.Installations.Services
{
    public class InstallationService : IInstallationService
    {
        private readonly ApplicationDbContext _context;

        public InstallationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedInstallationResponse> GetInstallationsAsync(
            string? search = null,
            string? category = null,
            string? status = null,
            string? contactId = null,
            string? tags = null,
            bool? hasWarranty = null,
            string? maintenanceFrequency = null,
            DateTime? createdFrom = null,
            DateTime? createdTo = null,
            int page = 1,
            int pageSize = 20,
            string sortBy = "created_at",
            string sortOrder = "desc"
        )
        {
            var query = _context.Installations.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(i =>
                    i.Name.ToLower().Contains(searchLower) ||
                    i.Model.ToLower().Contains(searchLower) ||
                    i.Manufacturer.ToLower().Contains(searchLower) ||
                    (i.SerialNumber != null && i.SerialNumber.ToLower().Contains(searchLower)) ||
                    (i.AssetTag != null && i.AssetTag.ToLower().Contains(searchLower))
                );
            }

            if (!string.IsNullOrEmpty(category))
                query = query.Where(i => i.Category == category);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(i => i.Status == status);

            if (!string.IsNullOrEmpty(contactId) && int.TryParse(contactId, out int contactIdInt))
                query = query.Where(i => i.ContactId == contactIdInt);

            if (hasWarranty.HasValue)
                query = query.Where(i => i.WarrantyHasWarranty == hasWarranty.Value);

            if (!string.IsNullOrEmpty(maintenanceFrequency))
                query = query.Where(i => i.MaintenanceFrequency == maintenanceFrequency);

            if (createdFrom.HasValue)
                query = query.Where(i => i.CreatedAt >= createdFrom.Value);

            if (createdTo.HasValue)
                query = query.Where(i => i.CreatedAt <= createdTo.Value);

            // Count total
            var total = await query.CountAsync();

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "name" => sortOrder.ToLower() == "asc" ? query.OrderBy(i => i.Name) : query.OrderByDescending(i => i.Name),
                "model" => sortOrder.ToLower() == "asc" ? query.OrderBy(i => i.Model) : query.OrderByDescending(i => i.Model),
                "category" => sortOrder.ToLower() == "asc" ? query.OrderBy(i => i.Category) : query.OrderByDescending(i => i.Category),
                "status" => sortOrder.ToLower() == "asc" ? query.OrderBy(i => i.Status) : query.OrderByDescending(i => i.Status),
                "updated_at" => sortOrder.ToLower() == "asc" ? query.OrderBy(i => i.UpdatedAt) : query.OrderByDescending(i => i.UpdatedAt),
                _ => sortOrder.ToLower() == "asc" ? query.OrderBy(i => i.CreatedAt) : query.OrderByDescending(i => i.CreatedAt)
            };

            // Apply pagination
            var installations = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var installationDtos = installations.Select(MapToDto).ToList();

            return new PaginatedInstallationResponse
            {
                Installations = installationDtos,
                Pagination = new PaginationInfo
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = total,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize),
                    HasNextPage = page * pageSize < total,
                    HasPreviousPage = page > 1
                }
            };
        }

        public async Task<InstallationDto?> GetInstallationByIdAsync(string id)
        {
            var installation = await _context.Installations
                .Include(i => i.MaintenanceHistories)
                .FirstOrDefaultAsync(i => i.Id == id);

            return installation == null ? null : MapToDto(installation);
        }

        public async Task<InstallationDto> CreateInstallationAsync(CreateInstallationDto createDto, string userId)
        {
            // Verify contact exists
            var contactExists = await _context.Contacts.AnyAsync(c => c.Id == createDto.ContactId);
            if (!contactExists)
                throw new KeyNotFoundException($"Contact with ID {createDto.ContactId} not found");

            var installationId = $"INST-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            var installationNumber = $"INST-{DateTime.UtcNow.Year}-{DateTime.UtcNow.Ticks.ToString().Substring(10)}";

            var installation = new Installation
            {
                Id = installationId,
                InstallationNumber = installationNumber,
                ContactId = createDto.ContactId,
                Name = createDto.Name,
                Model = createDto.Model,
                Manufacturer = createDto.Manufacturer,
                SerialNumber = createDto.SerialNumber,
                AssetTag = createDto.AssetTag,
                Category = createDto.Category,
                Type = createDto.Type,
                Status = createDto.Status,
                LocationAddress = createDto.Location?.Address,
                LocationCity = createDto.Location?.City,
                LocationState = createDto.Location?.State,
                LocationCountry = createDto.Location?.Country,
                LocationZipCode = createDto.Location?.ZipCode,
                LocationLatitude = createDto.Location?.Latitude,
                LocationLongitude = createDto.Location?.Longitude,
                SpecificationsProcessor = createDto.Specifications?.Processor,
                SpecificationsRam = createDto.Specifications?.Ram,
                SpecificationsStorage = createDto.Specifications?.Storage,
                SpecificationsOperatingSystem = createDto.Specifications?.OperatingSystem,
                SpecificationsOsVersion = createDto.Specifications?.OsVersion,
                WarrantyHasWarranty = createDto.Warranty?.HasWarranty ?? false,
                WarrantyFrom = createDto.Warranty?.WarrantyFrom,
                WarrantyTo = createDto.Warranty?.WarrantyTo,
                WarrantyProvider = createDto.Warranty?.WarrantyProvider,
                WarrantyType = createDto.Warranty?.WarrantyType,
                MaintenanceLastDate = createDto.Maintenance?.LastMaintenanceDate,
                MaintenanceNextDate = createDto.Maintenance?.NextMaintenanceDate,
                MaintenanceFrequency = createDto.Maintenance?.MaintenanceFrequency,
                MaintenanceNotes = createDto.Maintenance?.MaintenanceNotes,
                ContactPrimaryName = createDto.Contact?.PrimaryContactName,
                ContactPrimaryPhone = createDto.Contact?.PrimaryContactPhone,
                ContactPrimaryEmail = createDto.Contact?.PrimaryContactEmail,
                ContactSecondaryName = createDto.Contact?.SecondaryContactName,
                ContactSecondaryPhone = createDto.Contact?.SecondaryContactPhone,
                ContactSecondaryEmail = createDto.Contact?.SecondaryContactEmail,
                Tags = createDto.Metadata?.Tags,
                CustomFields = createDto.Metadata?.CustomFields != null ? JsonSerializer.Serialize(createDto.Metadata.CustomFields) : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            _context.Installations.Add(installation);
            await _context.SaveChangesAsync();

            var createdInstallation = await GetInstallationByIdAsync(installationId);
            return createdInstallation!;
        }

        public async Task<InstallationDto> UpdateInstallationAsync(string id, UpdateInstallationDto updateDto, string userId)
        {
            var installation = await _context.Installations.FindAsync(id);
            if (installation == null)
                throw new KeyNotFoundException($"Installation with ID {id} not found");

            // Update only provided fields
            if (updateDto.Name != null) installation.Name = updateDto.Name;
            if (updateDto.Model != null) installation.Model = updateDto.Model;
            if (updateDto.Manufacturer != null) installation.Manufacturer = updateDto.Manufacturer;
            if (updateDto.SerialNumber != null) installation.SerialNumber = updateDto.SerialNumber;
            if (updateDto.AssetTag != null) installation.AssetTag = updateDto.AssetTag;
            if (updateDto.Category != null) installation.Category = updateDto.Category;
            if (updateDto.Type != null) installation.Type = updateDto.Type;
            if (updateDto.Status != null) installation.Status = updateDto.Status;

            if (updateDto.Location != null)
            {
                installation.LocationAddress = updateDto.Location.Address;
                installation.LocationCity = updateDto.Location.City;
                installation.LocationState = updateDto.Location.State;
                installation.LocationCountry = updateDto.Location.Country;
                installation.LocationZipCode = updateDto.Location.ZipCode;
                installation.LocationLatitude = updateDto.Location.Latitude;
                installation.LocationLongitude = updateDto.Location.Longitude;
            }

            if (updateDto.Specifications != null)
            {
                installation.SpecificationsProcessor = updateDto.Specifications.Processor;
                installation.SpecificationsRam = updateDto.Specifications.Ram;
                installation.SpecificationsStorage = updateDto.Specifications.Storage;
                installation.SpecificationsOperatingSystem = updateDto.Specifications.OperatingSystem;
                installation.SpecificationsOsVersion = updateDto.Specifications.OsVersion;
            }

            if (updateDto.Warranty != null)
            {
                installation.WarrantyHasWarranty = updateDto.Warranty.HasWarranty;
                installation.WarrantyFrom = updateDto.Warranty.WarrantyFrom;
                installation.WarrantyTo = updateDto.Warranty.WarrantyTo;
                installation.WarrantyProvider = updateDto.Warranty.WarrantyProvider;
                installation.WarrantyType = updateDto.Warranty.WarrantyType;
            }

            if (updateDto.Maintenance != null)
            {
                installation.MaintenanceLastDate = updateDto.Maintenance.LastMaintenanceDate;
                installation.MaintenanceNextDate = updateDto.Maintenance.NextMaintenanceDate;
                installation.MaintenanceFrequency = updateDto.Maintenance.MaintenanceFrequency;
                installation.MaintenanceNotes = updateDto.Maintenance.MaintenanceNotes;
            }

            if (updateDto.Contact != null)
            {
                installation.ContactPrimaryName = updateDto.Contact.PrimaryContactName;
                installation.ContactPrimaryPhone = updateDto.Contact.PrimaryContactPhone;
                installation.ContactPrimaryEmail = updateDto.Contact.PrimaryContactEmail;
                installation.ContactSecondaryName = updateDto.Contact.SecondaryContactName;
                installation.ContactSecondaryPhone = updateDto.Contact.SecondaryContactPhone;
                installation.ContactSecondaryEmail = updateDto.Contact.SecondaryContactEmail;
            }

            if (updateDto.Metadata != null)
            {
                installation.Tags = updateDto.Metadata.Tags;
                installation.CustomFields = updateDto.Metadata.CustomFields != null ? JsonSerializer.Serialize(updateDto.Metadata.CustomFields) : null;
            }

            installation.UpdatedAt = DateTime.UtcNow;
            installation.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            var updatedInstallation = await GetInstallationByIdAsync(id);
            return updatedInstallation!;
        }

        public async Task<bool> DeleteInstallationAsync(string id)
        {
            var installation = await _context.Installations.FindAsync(id);
            if (installation == null)
                return false;

            _context.Installations.Remove(installation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<MaintenanceHistoryDto>> GetMaintenanceHistoryAsync(string installationId, int page = 1, int pageSize = 20)
        {
            var histories = await _context.MaintenanceHistories
                .Where(m => m.InstallationId == installationId)
                .OrderByDescending(m => m.MaintenanceDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return histories.Select(MapMaintenanceHistoryToDto).ToList();
        }

        public async Task<MaintenanceHistoryDto> AddMaintenanceHistoryAsync(string installationId, MaintenanceHistoryDto historyDto, string userId)
        {
            var installation = await _context.Installations.FindAsync(installationId);
            if (installation == null)
                throw new KeyNotFoundException($"Installation with ID {installationId} not found");

            var history = new MaintenanceHistory
            {
                Id = $"MH-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                InstallationId = installationId,
                MaintenanceDate = historyDto.MaintenanceDate,
                MaintenanceType = historyDto.MaintenanceType ?? "preventive",
                Description = historyDto.Description,
                Technician = historyDto.Technician,
                Duration = historyDto.Duration,
                Notes = historyDto.Notes,
                NextScheduledDate = historyDto.NextScheduledDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.MaintenanceHistories.Add(history);
            await _context.SaveChangesAsync();

            return MapMaintenanceHistoryToDto(history);
        }

        private InstallationDto MapToDto(Installation installation)
        {
            var customFields = !string.IsNullOrEmpty(installation.CustomFields)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(installation.CustomFields)
                : null;

            return new InstallationDto
            {
                Id = installation.Id,
                InstallationNumber = installation.InstallationNumber,
                ContactId = installation.ContactId,
                Name = installation.Name,
                Model = installation.Model,
                Manufacturer = installation.Manufacturer,
                SerialNumber = installation.SerialNumber,
                AssetTag = installation.AssetTag,
                Category = installation.Category,
                Type = installation.Type,
                Status = installation.Status,
                Location = new LocationDto
                {
                    Address = installation.LocationAddress,
                    City = installation.LocationCity,
                    State = installation.LocationState,
                    Country = installation.LocationCountry,
                    ZipCode = installation.LocationZipCode,
                    Latitude = installation.LocationLatitude,
                    Longitude = installation.LocationLongitude
                },
                Specifications = new SpecificationsDto
                {
                    Processor = installation.SpecificationsProcessor,
                    Ram = installation.SpecificationsRam,
                    Storage = installation.SpecificationsStorage,
                    OperatingSystem = installation.SpecificationsOperatingSystem,
                    OsVersion = installation.SpecificationsOsVersion
                },
                Warranty = new WarrantyDto
                {
                    HasWarranty = installation.WarrantyHasWarranty,
                    WarrantyFrom = installation.WarrantyFrom,
                    WarrantyTo = installation.WarrantyTo,
                    WarrantyProvider = installation.WarrantyProvider,
                    WarrantyType = installation.WarrantyType
                },
                Maintenance = new MaintenanceDto
                {
                    LastMaintenanceDate = installation.MaintenanceLastDate,
                    NextMaintenanceDate = installation.MaintenanceNextDate,
                    MaintenanceFrequency = installation.MaintenanceFrequency,
                    MaintenanceNotes = installation.MaintenanceNotes
                },
                Contact = new ContactInfoDto
                {
                    PrimaryContactName = installation.ContactPrimaryName,
                    PrimaryContactPhone = installation.ContactPrimaryPhone,
                    PrimaryContactEmail = installation.ContactPrimaryEmail,
                    SecondaryContactName = installation.ContactSecondaryName,
                    SecondaryContactPhone = installation.ContactSecondaryPhone,
                    SecondaryContactEmail = installation.ContactSecondaryEmail
                },
                Metadata = new MetadataDto
                {
                    Tags = installation.Tags,
                    CustomFields = customFields
                },
                CreatedAt = installation.CreatedAt,
                UpdatedAt = installation.UpdatedAt,
                CreatedBy = installation.CreatedBy,
                UpdatedBy = installation.UpdatedBy
            };
        }

        private MaintenanceHistoryDto MapMaintenanceHistoryToDto(MaintenanceHistory history)
        {
            return new MaintenanceHistoryDto
            {
                Id = history.Id,
                InstallationId = history.InstallationId,
                MaintenanceDate = history.MaintenanceDate,
                MaintenanceType = history.MaintenanceType,
                Description = history.Description,
                Technician = history.Technician,
                Duration = history.Duration,
                Notes = history.Notes,
                NextScheduledDate = history.NextScheduledDate,
                CreatedAt = history.CreatedAt,
                CreatedBy = history.CreatedBy
            };
        }
    }
}
