# REWIND — Bug Evidence Recorder for QA

**Tagline:** Every bug has a past. Capture it before it disappears.

REWIND is a local-first desktop application that continuously maintains a rolling recording of your screen during manual testing. When you encounter a bug, preserve what already happened with zero need to reproduce.

---

## Features

- **Continuous Rolling Buffer**: Retains the last 30–60 seconds of screen history with bounded resource usage (auto-pruned).
- **Instant Bug Preservation**: Save the preceding footage immediately after discovering unexpected behavior.
- **Floating HUD Overlay**: Minimalist draggable overlay accessible while testing target web, desktop, or mobile applications.
- **Interactive Timeline Editor**: Scrub recent footage, adjust range boundaries, and preview before saving.
- **Local-First SQLite Persistence**: Stores sessions, bug cards, notes, and MP4 evidence on your local machine.
- **Consolidated Markdown QA Reports**: One-click generation of structured bug reports ready for Jira, GitHub, Linear, or Slack.
- **AI-Assisted Documentation**: Optional rule-based offline visual analysis suggesting titles, observed behavior, and candidate reproduction steps.

---

## Technical Architecture

- **Desktop Shell/UI**: .NET 8 (WPF) with modern dark aesthetic
- **Capture Engine**: High-performance Windows screen capture with bounded circular buffer
- **Video Assembly**: `ffmpeg` H.264/MP4 demuxing and encoding
- **Persistence**: SQLite (`Microsoft.Data.Sqlite`)
- **Reporting**: Deterministic Markdown report generator

---

## Getting Started

### Prerequisites

- Windows 10/11
- .NET 8 SDK
- `ffmpeg` in system `PATH`

### Build and Run

```powershell
# Restore & build
dotnet build Rewind.sln

# Run the app
dotnet run --project Rewind.App/Rewind.App.csproj
```
