using System;

namespace MyApi.Modules.Dispatches.Models
{
    public class Note
    {
        public string Id { get; set; } = null!;
        public string DispatchId { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Category { get; set; }
        public string? Priority { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
