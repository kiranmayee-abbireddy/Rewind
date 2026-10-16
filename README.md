# REWIND — Bug Evidence Recorder for QA

**Tagline:** Every bug has a past. Capture it before it disappears.

REWIND is a local-first desktop application that continuously maintains a rolling recording of your screen during manual testing. When you encounter a bug, preserve what already happened with zero need to reproduce.

---

## Key Features

- **Continuous Rolling Buffer**: Retains the last 15–180 seconds of screen history with bounded resource usage (auto-pruned in the background).
- **Instant Bug Preservation**: Save the preceding footage immediately after discovering unexpected behavior.
- **Floating HUD Overlay**: Minimalist draggable overlay with pulsating recording pulse, live time counter, and instant **⚡ Preserve Bug** action.
- **Global Win32 Hotkey (`Ctrl+Shift+R`)**: Trigger preservation instantly from any active application under test without window switching.
- **Interactive Timeline Scrubber**: Scrub recent footage with thumbnail preview and quick duration presets (15s, 30s, 60s).
- **Sensitive Data Redaction**: Drag and drop black masking rectangles directly on the preview to permanently redact passwords, auth tokens, or PII before MP4 encoding.
- **In-App Media Player**: Embedded video player with hardware acceleration, play/pause controls, and interactive timeline scrubbing in the bug detail view.
- **Local-First SQLite Persistence**: Stores sessions, bug cards, notes, and MP4 evidence on your local machine (`rewind.db`).
- **Multi-Format QA Reports**: One-click generation of structured bug reports in **Markdown (`.md`)**, **Jira Markup**, or **JSON Data** format.
- **Multi-Provider AI Documentation**:
  - **Offline Local Rule Assistant**: Operates completely offline with zero telemetry.
  - **Google Gemini Multimodal Vision (Opt-in)**: Analyzes evidence thumbnails to draft bug titles, observed behavior, and candidate reproduction steps.
- **Crash Recovery & Reliability**: Automatically detects and recovers interrupted sessions after abnormal shutdowns.
- **Disk Storage Footprint Meter**: Real-time storage consumption meter with an **Open Folder** action to inspect evidence on disk.

---

## Technical Architecture

- **Desktop Shell/UI**: .NET 8 (WPF) with custom dark-mode theme
- **Capture Engine**: High-performance Windows screen capture with bounded circular buffer
- **Video Assembly**: `ffmpeg` H.264/MP4 demuxing and `drawbox` redaction filter chain
- **Persistence**: SQLite (`Microsoft.Data.Sqlite`)
- **Reporting**: Deterministic multi-format report generator (Markdown, Jira, JSON)
- **Automated Tests**: xUnit test suite (`Rewind.Tests`) with 100% pass rate

---

## Getting Started

### Prerequisites

- Windows 10/11 (x64)
- .NET 8 SDK
- `ffmpeg` in system `PATH`

### Build and Run

```powershell
# Restore & build solution
dotnet build Rewind.sln

# Run the test suite
dotnet test Rewind.sln

# Launch the desktop app
dotnet run --project Rewind.App/Rewind.App.csproj
```

### Self-Contained Release Publishing

To publish a standalone executable that runs without requiring a pre-installed .NET runtime:

```powershell
dotnet publish Rewind.App/Rewind.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./dist
```
