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

            // Schema migration: Add soft-delete columns if they do not exist
            try
            {
                var alterSessions = conn.CreateCommand();
                alterSessions.CommandText = "ALTER TABLE Sessions ADD COLUMN IsDeleted INTEGER DEFAULT 0; ALTER TABLE Sessions ADD COLUMN DeletedAt TEXT;";
                alterSessions.ExecuteNonQuery();
            }
            catch { }

            try
            {
                var alterBugs = conn.CreateCommand();
                alterBugs.CommandText = "ALTER TABLE Bugs ADD COLUMN IsDeleted INTEGER DEFAULT 0; ALTER TABLE Bugs ADD COLUMN DeletedAt TEXT;";
                alterBugs.ExecuteNonQuery();
            }
            catch { }

            // Automatically purge items in Recycle Bin that are older than 30 days
            PurgeExpiredBinItems(conn);
        }

        private void PurgeExpiredBinItems(SqliteConnection conn)
        {
            try
            {
                var cutoff = DateTime.UtcNow.AddDays(-30).ToString("o");
                
                // Get expired bugs to clean up their video/thumb files
                var getBugsCmd = conn.CreateCommand();
                getBugsCmd.CommandText = "SELECT ClipPath, ThumbnailPath FROM Bugs WHERE IsDeleted = 1 AND DeletedAt IS NOT NULL AND DeletedAt < $cutoff;";
                getBugsCmd.Parameters.AddWithValue("$cutoff", cutoff);
                using (var reader = getBugsCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var clip = reader.IsDBNull(0) ? null : reader.GetString(0);
                        var thumb = reader.IsDBNull(1) ? null : reader.GetString(1);
                        try { if (!string.IsNullOrEmpty(clip) && File.Exists(clip)) File.Delete(clip); } catch { }
                        try { if (!string.IsNullOrEmpty(thumb) && File.Exists(thumb)) File.Delete(thumb); } catch { }
                    }
                }

                // Delete expired bugs and sessions
                var purgeCmd = conn.CreateCommand();
                purgeCmd.CommandText = @"
                    DELETE FROM Bugs WHERE IsDeleted = 1 AND DeletedAt IS NOT NULL AND DeletedAt < $cutoff;
                    DELETE FROM Sessions WHERE IsDeleted = 1 AND DeletedAt IS NOT NULL AND DeletedAt < $cutoff;
                ";
                purgeCmd.Parameters.AddWithValue("$cutoff", cutoff);
                purgeCmd.ExecuteNonQuery();
            }
            catch { }
        }

        public void SaveSession(Session session)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Sessions (Id, Title, CreatedAt, StartedAt, EndedAt, CaptureSource, CaptureSettings, Status, Notes, IsDeleted, DeletedAt)
                VALUES ($id, $title, $createdAt, $startedAt, $endedAt, $captureSource, $captureSettings, $status, $notes, $isDel, $delAt)
                ON CONFLICT(Id) DO UPDATE SET
                    Title = $title,
                    StartedAt = $startedAt,
                    EndedAt = $endedAt,
                    CaptureSource = $captureSource,
                    CaptureSettings = $captureSettings,
                    Status = $status,
                    Notes = $notes,
                    IsDeleted = $isDel,
                    DeletedAt = $delAt;
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
            cmd.Parameters.AddWithValue("$isDel", session.IsDeleted ? 1 : 0);
            cmd.Parameters.AddWithValue("$delAt", (object?)session.DeletedAt?.ToString("o") ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public List<Session> GetSessions(bool includeDeleted = false)
        {
            var list = new List<Session>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = includeDeleted
                ? "SELECT Id, Title, CreatedAt, StartedAt, EndedAt, CaptureSource, CaptureSettings, Status, Notes, IsDeleted, DeletedAt FROM Sessions WHERE IsDeleted = 1 ORDER BY DeletedAt DESC;"
                : "SELECT Id, Title, CreatedAt, StartedAt, EndedAt, CaptureSource, CaptureSettings, Status, Notes, IsDeleted, DeletedAt FROM Sessions WHERE IsDeleted = 0 OR IsDeleted IS NULL ORDER BY CreatedAt DESC;";

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
                    Notes = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    IsDeleted = !reader.IsDBNull(9) && reader.GetInt32(9) == 1,
                    DeletedAt = reader.IsDBNull(10) ? null : DateTime.Parse(reader.GetString(10))
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
                INSERT INTO Bugs (Id, SessionId, Title, CreatedAt, ClipStartSeconds, ClipEndSeconds, ClipPath, ThumbnailPath, ObservedBehavior, ExpectedBehavior, ReproductionSteps, Notes, Tags, Status, AiAssisted, AiSuggestions, IsDeleted, DeletedAt)
                VALUES ($id, $sessionId, $title, $createdAt, $clipStart, $clipEnd, $clipPath, $thumb, $obs, $exp, $repro, $notes, $tags, $status, $ai, $sugg, $isDel, $delAt)
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
                    AiSuggestions = $sugg,
                    IsDeleted = $isDel,
                    DeletedAt = $delAt;
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
            cmd.Parameters.AddWithValue("$isDel", bug.IsDeleted ? 1 : 0);
            cmd.Parameters.AddWithValue("$delAt", (object?)bug.DeletedAt?.ToString("o") ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public List<Bug> GetBugsForSession(string sessionId, bool includeDeleted = false)
        {
            var list = new List<Bug>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = includeDeleted
                ? "SELECT Id, SessionId, Title, CreatedAt, ClipStartSeconds, ClipEndSeconds, ClipPath, ThumbnailPath, ObservedBehavior, ExpectedBehavior, ReproductionSteps, Notes, Tags, Status, AiAssisted, AiSuggestions, IsDeleted, DeletedAt FROM Bugs WHERE SessionId = $sessionId AND IsDeleted = 1 ORDER BY DeletedAt DESC;"
                : "SELECT Id, SessionId, Title, CreatedAt, ClipStartSeconds, ClipEndSeconds, ClipPath, ThumbnailPath, ObservedBehavior, ExpectedBehavior, ReproductionSteps, Notes, Tags, Status, AiAssisted, AiSuggestions, IsDeleted, DeletedAt FROM Bugs WHERE SessionId = $sessionId AND (IsDeleted = 0 OR IsDeleted IS NULL) ORDER BY CreatedAt ASC;";
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
                    AiSuggestions = reader.IsDBNull(15) ? "" : reader.GetString(15),
                    IsDeleted = !reader.IsDBNull(16) && reader.GetInt32(16) == 1,
                    DeletedAt = reader.IsDBNull(17) ? null : DateTime.Parse(reader.GetString(17))
                });
            }
            return list;
        }

        public List<Bug> GetDeletedBugs()
        {
            var list = new List<Bug>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, SessionId, Title, CreatedAt, ClipStartSeconds, ClipEndSeconds, ClipPath, ThumbnailPath, ObservedBehavior, ExpectedBehavior, ReproductionSteps, Notes, Tags, Status, AiAssisted, AiSuggestions, IsDeleted, DeletedAt FROM Bugs WHERE IsDeleted = 1 ORDER BY DeletedAt DESC;";

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
                    AiSuggestions = reader.IsDBNull(15) ? "" : reader.GetString(15),
                    IsDeleted = !reader.IsDBNull(16) && reader.GetInt32(16) == 1,
                    DeletedAt = reader.IsDBNull(17) ? null : DateTime.Parse(reader.GetString(17))
                });
            }
            return list;
        }

        public int GetRecycleBinCount()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT (SELECT COUNT(*) FROM Sessions WHERE IsDeleted = 1) + (SELECT COUNT(*) FROM Bugs WHERE IsDeleted = 1);";
            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        public void SoftDeleteBug(string bugId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Bugs SET IsDeleted = 1, DeletedAt = $delAt WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", bugId);
            cmd.Parameters.AddWithValue("$delAt", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        public void RestoreBug(string bugId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Bugs SET IsDeleted = 0, DeletedAt = NULL WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", bugId);
            cmd.ExecuteNonQuery();
        }

        public void SoftDeleteSession(string sessionId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var now = DateTime.UtcNow.ToString("o");
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Sessions SET IsDeleted = 1, DeletedAt = $delAt WHERE Id = $id;
                UPDATE Bugs SET IsDeleted = 1, DeletedAt = $delAt WHERE SessionId = $id;
            ";
            cmd.Parameters.AddWithValue("$id", sessionId);
            cmd.Parameters.AddWithValue("$delAt", now);
            cmd.ExecuteNonQuery();
        }

        public void RestoreSession(string sessionId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Sessions SET IsDeleted = 0, DeletedAt = NULL WHERE Id = $id;
                UPDATE Bugs SET IsDeleted = 0, DeletedAt = NULL WHERE SessionId = $id;
            ";
            cmd.Parameters.AddWithValue("$id", sessionId);
            cmd.ExecuteNonQuery();
        }

        public void PermanentlyDeleteBug(string bugId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            // Retrieve clip path to remove file on disk
            var pathCmd = conn.CreateCommand();
            pathCmd.CommandText = "SELECT ClipPath, ThumbnailPath FROM Bugs WHERE Id = $id;";
            pathCmd.Parameters.AddWithValue("$id", bugId);
            using (var reader = pathCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    var clip = reader.IsDBNull(0) ? null : reader.GetString(0);
                    var thumb = reader.IsDBNull(1) ? null : reader.GetString(1);
                    try { if (!string.IsNullOrEmpty(clip) && File.Exists(clip)) File.Delete(clip); } catch { }
                    try { if (!string.IsNullOrEmpty(thumb) && File.Exists(thumb)) File.Delete(thumb); } catch { }
                }
            }

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Bugs WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", bugId);
            cmd.ExecuteNonQuery();
        }

        public void PermanentlyDeleteSession(string sessionId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            // Remove session directory on disk
            try
            {
                var sessionClipsDir = Path.Combine(BaseDirectory, "sessions", sessionId);
                if (Directory.Exists(sessionClipsDir))
                {
                    Directory.Delete(sessionClipsDir, recursive: true);
                }
            }
            catch { }

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Sessions WHERE Id = $id; DELETE FROM Bugs WHERE SessionId = $id;";
            cmd.Parameters.AddWithValue("$id", sessionId);
            cmd.ExecuteNonQuery();
        }

        public void EmptyRecycleBin()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            // Clean up files for deleted bugs
            var getBugsCmd = conn.CreateCommand();
            getBugsCmd.CommandText = "SELECT ClipPath, ThumbnailPath FROM Bugs WHERE IsDeleted = 1;";
            using (var reader = getBugsCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var clip = reader.IsDBNull(0) ? null : reader.GetString(0);
                    var thumb = reader.IsDBNull(1) ? null : reader.GetString(1);
                    try { if (!string.IsNullOrEmpty(clip) && File.Exists(clip)) File.Delete(clip); } catch { }
                    try { if (!string.IsNullOrEmpty(thumb) && File.Exists(thumb)) File.Delete(thumb); } catch { }
                }
            }

            // Clean up session folders for deleted sessions
            var getSessionsCmd = conn.CreateCommand();
            getSessionsCmd.CommandText = "SELECT Id FROM Sessions WHERE IsDeleted = 1;";
            using (var reader = getSessionsCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var sId = reader.GetString(0);
                    try
                    {
                        var sessionClipsDir = Path.Combine(BaseDirectory, "sessions", sId);
                        if (Directory.Exists(sessionClipsDir)) Directory.Delete(sessionClipsDir, recursive: true);
                    }
                    catch { }
                }
            }

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                DELETE FROM Bugs WHERE IsDeleted = 1;
                DELETE FROM Sessions WHERE IsDeleted = 1;
            ";
            cmd.ExecuteNonQuery();
        }

        // Backward compatibility wrappers
        public void DeleteBug(string bugId) => SoftDeleteBug(bugId);
        public void DeleteSession(string sessionId) => SoftDeleteSession(sessionId);

        public void SaveAppSettings(AppSettings settings)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(settings);
                using var conn = new SqliteConnection(_connectionString);
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Settings (Key, Value) VALUES ('AppSettings', $val)
                    ON CONFLICT(Key) DO UPDATE SET Value = $val;
                ";
                cmd.Parameters.AddWithValue("$val", json);
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        public AppSettings LoadAppSettings()
        {
            try
            {
                using var conn = new SqliteConnection(_connectionString);
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Value FROM Settings WHERE Key = 'AppSettings' LIMIT 1;";
                var val = cmd.ExecuteScalar()?.ToString();
                if (!string.IsNullOrWhiteSpace(val))
                {
                    var settings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(val);
                    if (settings != null) return settings;
                }
            }
            catch { }
            return new AppSettings();
        }
    }
}
