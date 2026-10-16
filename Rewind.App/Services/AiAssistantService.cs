using System;
using System.IO;
using System.Threading.Tasks;

namespace Rewind.App.Services
{
    public interface IAiAssistantService
    {
        bool IsEnabled { get; set; }
        string ProviderName { get; }
        Task<(string Title, string ObservedBehavior, string ReproSteps, string Suggestions)> AnalyzeClipAsync(string clipPath, double durationSeconds);
    }

    public class RuleBasedLocalAiAssistant : IAiAssistantService
    {
        public bool IsEnabled { get; set; } = true;
        public string ProviderName => "REWIND Local Rule Engine (Offline)";

        public async Task<(string Title, string ObservedBehavior, string ReproSteps, string Suggestions)> AnalyzeClipAsync(string clipPath, double durationSeconds)
        {
            // Simulate local async visual analysis
            await Task.Delay(300);

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var filename = Path.GetFileName(clipPath);

            var title = $"Unexpected UI state transition detected @ {timestamp}";
            var observed = $"Anomalous interaction or layout change recorded in clip ({durationSeconds:0.0}s). Screen visual activity captured in {filename}.";
            var repro = $"1. Open application under test.\n2. Replicate actions performed between {durationSeconds:0.0}s prior to bug capture.\n3. Observe UI state failure.";
            var suggestions = "Suggested severity: Medium (UI/Interaction). Review candidate repro steps and verify if console errors or network timeouts accompanied this transition.";

            return (title, observed, repro, suggestions);
        }
    }
}
