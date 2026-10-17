using System;

namespace Rewind.App.Models
{
    public class Bug
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SessionId { get; set; } = string.Empty;
        public string Title { get; set; } = "Untitled Bug";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public double ClipStartSeconds { get; set; }
        public double ClipEndSeconds { get; set; }
        public string ClipPath { get; set; } = string.Empty;
        public string ThumbnailPath { get; set; } = string.Empty;
        public string ObservedBehavior { get; set; } = string.Empty;
        public string ExpectedBehavior { get; set; } = string.Empty;
        public string ReproductionSteps { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Tags { get; set; } = "UI, Regression";
        public string Status { get; set; } = "Open"; // Open, Verified, Fixed, Ignored
        public bool AiAssisted { get; set; } = false;
        public string AiSuggestions { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public double DurationSeconds => Math.Max(0, ClipEndSeconds - ClipStartSeconds);
    }
}
