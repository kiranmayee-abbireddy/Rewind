using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using Rewind.App.Models;

namespace Rewind.App.Services
{
    public class DatabaseService
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public DatabaseService(string? customDir = null)
        {
            var baseDir = customDir ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Rewind");
            Directory.CreateDirectory(baseDir);
            _dbPath = Path.Combine(baseDir, "rewind.db");
            _connectionString = $"Data Source={_dbPath}";
            InitializeDatabase();
        }

        public string BaseDirectory => Path.GetDirectoryName(_dbPath)!;

        private void InitializeDatabase()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Sessions (
                    Id TEXT PRIMARY KEY,
                    Title TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    StartedAt TEXT,
                    EndedAt TEXT,
                    CaptureSource TEXT,
                    CaptureSettings TEXT,
                    Status TEXT NOT NULL,
                    Notes TEXT
                );

                CREATE TABLE IF NOT EXISTS Bugs (
                    Id TEXT PRIMARY KEY,
                    SessionId TEXT NOT NULL,
                    Title TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    ClipStartSeconds REAL NOT NULL,
                    ClipEndSeconds REAL NOT NULL,
                    ClipPath TEXT,
                    ThumbnailPath TEXT,
                    ObservedBehavior TEXT,
                    ExpectedBehavior TEXT,
                    ReproductionSteps TEXT,
                    Notes TEXT,
                    Tags TEXT,
                    Status TEXT,
                    AiAssisted INTEGER,
                    AiSuggestions TEXT,
                    FOREIGN KEY(SessionId) REFERENCES Sessions(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS Settings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT NOT NULL
                );
            ";
            cmd.ExecuteNonQuery();
        }

        public void SaveSession(Session session)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Sessions (Id, Title, CreatedAt, StartedAt, EndedAt, CaptureSource, CaptureSettings, Status, Notes)
                VALUES ($id, $title, $createdAt, $startedAt, $endedAt, $captureSource, $captureSettings, $status, $notes)
                ON CONFLICT(Id) DO UPDATE SET
                    Title = $title,
                    StartedAt = $startedAt,
                    EndedAt = $endedAt,
                    CaptureSource = $captureSource,
                    CaptureSettings = $captureSettings,
                    Status = $status,
                    Notes = $notes;
            ";
            cmd.Parameters.AddWithValue("$id", session.Id);
            cmd.Parameters.AddWithValue("$title", session.Title);
            cmd.Parameters.AddWithValue("$createdAt", session.CreatedAt.ToString("o"));
            cmd.Parameters.AddWithValue("$startedAt", (object?)session.StartedAt?.ToString("o") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$endedAt", (object?)session.EndedAt?.ToString("o") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$captureSource", session.CaptureSource);
            cmd.Parameters.AddWithValue("$captureSettings", session.CaptureSettings);
            cmd.Parameters.AddWithValue("$status", session.Status);
            cmd.Parameters.AddWithValue("$notes", session.Notes ?? string.Empty);
            cmd.ExecuteNonQuery();
        }

        public List<Session> GetSessions()
        {
            var list = new List<Session>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Title, CreatedAt, StartedAt, EndedAt, CaptureSource, CaptureSettings, Status, Notes FROM Sessions ORDER BY CreatedAt DESC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Session
                {
                    Id = reader.GetString(0),
                    Title = reader.GetString(1),
                    CreatedAt = DateTime.Parse(reader.GetString(2)),
                    StartedAt = reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3)),
                    EndedAt = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
                    CaptureSource = reader.IsDBNull(5) ? "Primary Display" : reader.GetString(5),
                    CaptureSettings = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    Status = reader.GetString(7),
                    Notes = reader.IsDBNull(8) ? "" : reader.GetString(8)
                });
            }
            return list;
        }

        public void SaveBug(Bug bug)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Bugs (Id, SessionId, Title, CreatedAt, ClipStartSeconds, ClipEndSeconds, ClipPath, ThumbnailPath, ObservedBehavior, ExpectedBehavior, ReproductionSteps, Notes, Tags, Status, AiAssisted, AiSuggestions)
                VALUES ($id, $sessionId, $title, $createdAt, $clipStart, $clipEnd, $clipPath, $thumb, $obs, $exp, $repro, $notes, $tags, $status, $ai, $sugg)
                ON CONFLICT(Id) DO UPDATE SET
                    Title = $title,
                    ClipStartSeconds = $clipStart,
                    ClipEndSeconds = $clipEnd,
                    ClipPath = $clipPath,
                    ThumbnailPath = $thumb,
                    ObservedBehavior = $obs,
                    ExpectedBehavior = $exp,
                    ReproductionSteps = $repro,
                    Notes = $notes,
                    Tags = $tags,
                    Status = $status,
                    AiAssisted = $ai,
                    AiSuggestions = $sugg;
            ";
            cmd.Parameters.AddWithValue("$id", bug.Id);
            cmd.Parameters.AddWithValue("$sessionId", bug.SessionId);
            cmd.Parameters.AddWithValue("$title", bug.Title);
            cmd.Parameters.AddWithValue("$createdAt", bug.CreatedAt.ToString("o"));
            cmd.Parameters.AddWithValue("$clipStart", bug.ClipStartSeconds);
            cmd.Parameters.AddWithValue("$clipEnd", bug.ClipEndSeconds);
            cmd.Parameters.AddWithValue("$clipPath", bug.ClipPath ?? string.Empty);
            cmd.Parameters.AddWithValue("$thumb", bug.ThumbnailPath ?? string.Empty);
            cmd.Parameters.AddWithValue("$obs", bug.ObservedBehavior ?? string.Empty);
            cmd.Parameters.AddWithValue("$exp", bug.ExpectedBehavior ?? string.Empty);
            cmd.Parameters.AddWithValue("$repro", bug.ReproductionSteps ?? string.Empty);
            cmd.Parameters.AddWithValue("$notes", bug.Notes ?? string.Empty);
            cmd.Parameters.AddWithValue("$tags", bug.Tags ?? string.Empty);
            cmd.Parameters.AddWithValue("$status", bug.Status ?? "Open");
            cmd.Parameters.AddWithValue("$ai", bug.AiAssisted ? 1 : 0);
            cmd.Parameters.AddWithValue("$sugg", bug.AiSuggestions ?? string.Empty);
            cmd.ExecuteNonQuery();
        }

        public List<Bug> GetBugsForSession(string sessionId)
        {
            var list = new List<Bug>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, SessionId, Title, CreatedAt, ClipStartSeconds, ClipEndSeconds, ClipPath, ThumbnailPath, ObservedBehavior, ExpectedBehavior, ReproductionSteps, Notes, Tags, Status, AiAssisted, AiSuggestions FROM Bugs WHERE SessionId = $sessionId ORDER BY CreatedAt ASC;";
            cmd.Parameters.AddWithValue("$sessionId", sessionId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Bug
                {
                    Id = reader.GetString(0),
                    SessionId = reader.GetString(1),
                    Title = reader.GetString(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    ClipStartSeconds = reader.GetDouble(4),
                    ClipEndSeconds = reader.GetDouble(5),
                    ClipPath = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    ThumbnailPath = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    ObservedBehavior = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    ExpectedBehavior = reader.IsDBNull(9) ? "" : reader.GetString(9),
                    ReproductionSteps = reader.IsDBNull(10) ? "" : reader.GetString(10),
                    Notes = reader.IsDBNull(11) ? "" : reader.GetString(11),
                    Tags = reader.IsDBNull(12) ? "" : reader.GetString(12),
                    Status = reader.IsDBNull(13) ? "Open" : reader.GetString(13),
                    AiAssisted = reader.GetInt32(14) == 1,
                    AiSuggestions = reader.IsDBNull(15) ? "" : reader.GetString(15)
                });
            }
            return list;
        }

        public void DeleteBug(string bugId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Bugs WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", bugId);
            cmd.ExecuteNonQuery();
        }

        public void DeleteSession(string sessionId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Sessions WHERE Id = $id; DELETE FROM Bugs WHERE SessionId = $id;";
            cmd.Parameters.AddWithValue("$id", sessionId);
            cmd.ExecuteNonQuery();
        }
    }
}
