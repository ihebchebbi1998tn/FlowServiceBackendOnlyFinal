using System;
using Microsoft.AspNetCore.Http;

namespace MyApi.Modules.Dispatches.DTOs
{
    public class AttachmentUploadResponseDto
    {
        public string Id { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public decimal FileSizeMb { get; set; }
        public string? Category { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
