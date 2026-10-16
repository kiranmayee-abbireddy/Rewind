using System;
using System.IO;
using Rewind.App.Models;
using Rewind.App.Services;
using Xunit;

namespace Rewind.Tests
{
    public class PersistenceTests : IDisposable
    {
        private readonly string _testDir;
        private readonly DatabaseService _dbService;

        public PersistenceTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "RewindTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);
            _dbService = new DatabaseService(_testDir);
        }

        [Fact]
        public void SessionAndBug_ShouldPersistAndReloadCorrectly()
        {
            // Arrange
            var session = new Session
            {
                Title = "Manual Regression Pass v1.0",
                CaptureSource = "Primary Display",
                Status = "Recording"
            };
            _dbService.SaveSession(session);

            var bug = new Bug
            {
                SessionId = session.Id,
                Title = "Button click fails to open modal",
                ClipStartSeconds = 10.0,
                ClipEndSeconds = 25.5,
                ObservedBehavior = "Modal does not render on screen",
                ExpectedBehavior = "Confirmation modal opens immediately",
                ReproductionSteps = "1. Navigate to Settings\n2. Click Reset",
                Tags = "UI, P0",
                Status = "Open",
                AiAssisted = true,
                AiSuggestions = "Check click event bindings"
            };
            _dbService.SaveBug(bug);

            // Act - Re-instantiate database service to verify cold read from disk
            var coldDb = new DatabaseService(_testDir);
            var sessions = coldDb.GetSessions();
            var bugs = coldDb.GetBugsForSession(session.Id);

            // Assert
            Assert.Single(sessions);
            Assert.Equal("Manual Regression Pass v1.0", sessions[0].Title);
            Assert.Equal("Recording", sessions[0].Status);

            Assert.Single(bugs);
            Assert.Equal("Button click fails to open modal", bugs[0].Title);
            Assert.Equal(15.5, bugs[0].DurationSeconds, 1);
            Assert.Equal("Confirmation modal opens immediately", bugs[0].ExpectedBehavior);
            Assert.True(bugs[0].AiAssisted);
        }

        [Fact]
        public void DeleteBug_ShouldRemoveBugFromDatabase()
        {
            var session = new Session { Title = "Session For Deletion" };
            _dbService.SaveSession(session);

            var bug = new Bug { SessionId = session.Id, Title = "Temporary Bug" };
            _dbService.SaveBug(bug);

            Assert.Single(_dbService.GetBugsForSession(session.Id));

            _dbService.DeleteBug(bug.Id);

            Assert.Empty(_dbService.GetBugsForSession(session.Id));
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testDir))
                {
                    Directory.Delete(_testDir, true);
                }
            }
            catch { }
        }
    }
}
