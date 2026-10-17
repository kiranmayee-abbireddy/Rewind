using System;

namespace Rewind.App.Models
{
    public class Session
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = $"Session {DateTime.Now:yyyy-MM-dd HH:mm}";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string CaptureSource { get; set; } = "Primary Display";
        public string CaptureSettings { get; set; } = "1080p @ 15fps, 60s buffer";
        public string Status { get; set; } = "Draft"; // Draft, Recording, Paused, Completed
        public string Notes { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
