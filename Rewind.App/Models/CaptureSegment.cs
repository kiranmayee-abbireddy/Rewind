using System;

namespace Rewind.App.Models
{
    public class CaptureSegment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SessionId { get; set; } = string.Empty;
        public double StartTimestampSeconds { get; set; }
        public double EndTimestampSeconds { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string State { get; set; } = "Temporary"; // Temporary, Pinned, Saved
        public string EncodingMetadata { get; set; } = "H.264/MP4";
    }

    public class AppSettings
    {
        public int BufferDurationSeconds { get; set; } = 60;
        public string CaptureSource { get; set; } = "Primary Display";
        public int FrameRate { get; set; } = 15;
        public string OutputResolution { get; set; } = "1920x1080";
        public string StorageDirectory { get; set; } = string.Empty;
        public int MaximumClipDurationSeconds { get; set; } = 60;
        public string AiProvider { get; set; } = "Ollama";
        public bool AiEnabled { get; set; } = true;
        public string OllamaEndpoint { get; set; } = "http://localhost:11434";
        public string OllamaModel { get; set; } = "gemma3:1b";
        public bool RecordMicrophone { get; set; } = false;
    }
}
