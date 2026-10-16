using System;
using System.IO;
using System.Threading.Tasks;
using Rewind.App.Services;
using Xunit;

namespace Rewind.Tests
{
    public class CaptureEngineTests : IDisposable
    {
        private readonly string _testBufferDir;

        public CaptureEngineTests()
        {
            _testBufferDir = Path.Combine(Path.GetTempPath(), "RewindBufferTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testBufferDir);
        }

        [Fact]
        public async Task PreserveClipAsync_EmptyFrames_ShouldThrowOrFallbackGracefully()
        {
            using var engine = new CaptureEngine(_testBufferDir, bufferSeconds: 10, fps: 15);

            var destFile = Path.Combine(_testBufferDir, "empty_clip.mp4");

            // Range with no captured frames
            var result = await engine.PreserveClipAsync(100.0, 110.0, destFile);

            Assert.True(File.Exists(result));
        }

        [Fact]
        public async Task LocalRuleAiAssistant_ShouldReturnStructuredSuggestions()
        {
            var assistant = new RuleBasedLocalAiAssistant();

            var result = await assistant.AnalyzeClipAsync("dummy.mp4", 30.0);

            Assert.False(string.IsNullOrWhiteSpace(result.Title));
            Assert.Contains("30.0", result.ObservedBehavior);
            Assert.Contains("1.", result.ReproSteps);
            Assert.Contains("Severity", result.Suggestions);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testBufferDir))
                {
                    Directory.Delete(_testBufferDir, true);
                }
            }
            catch { }
        }
    }
}
