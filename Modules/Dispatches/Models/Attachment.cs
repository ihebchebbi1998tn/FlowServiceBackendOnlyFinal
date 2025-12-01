using System;

namespace MyApi.Modules.Dispatches.Models
{
    public class Attachment
    {
        public string Id { get; set; } = null!;
        public string DispatchId { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public decimal FileSizeMb { get; set; }
        public string? Category { get; set; }
        public string? UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? StoragePath { get; set; }
    }
}
