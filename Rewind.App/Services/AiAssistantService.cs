using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rewind.App.Services
{
    public interface IAiAssistantService
    {
        bool IsEnabled { get; set; }
        string ProviderName { get; }
        Task<(string Title, string ObservedBehavior, string ReproSteps, string Suggestions)> AnalyzeClipAsync(string clipPath, double durationSeconds);
        Task<string> SummarizeSessionReportAsync(string sessionTitle, int bugCount, string bugTitlesSummary);
    }

    public class RuleBasedLocalAiAssistant : IAiAssistantService
    {
        public bool IsEnabled { get; set; } = true;
        public string ProviderName => "REWIND Local Rule Engine (Offline)";

        public async Task<(string Title, string ObservedBehavior, string ReproSteps, string Suggestions)> AnalyzeClipAsync(string clipPath, double durationSeconds)
        {
            await Task.Delay(250);

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var filename = Path.GetFileName(clipPath);

            var title = $"Unexpected UI state transition detected @ {timestamp}";
            var observed = $"Anomalous interaction or layout change recorded in clip ({durationSeconds:0.0}s). Screen visual activity captured in {filename}.";
            var repro = $"1. Launch application under test.\n2. Navigate to screen active during discovery.\n3. Execute actions recorded in the final {durationSeconds:0.0}s of the clip.\n4. Observe discrepancy with expected state.";
            var suggestions = "Candidate Severity: Moderate (Visual / Behavioral discrepancy).\n• Verify if related console errors, network drops, or rendering glitches occurred.\n• Check if responsive breakpoint or layout boundary condition triggered this anomaly.";

            return (title, observed, repro, suggestions);
        }

        public async Task<string> SummarizeSessionReportAsync(string sessionTitle, int bugCount, string bugTitlesSummary)
        {
            await Task.Delay(200);

            if (bugCount == 0)
            {
                return "Session concluded with no defects preserved. All observed workflows functioned within expected behavioral bounds.";
            }

            return $"Executive Summary: Completed exploratory QA pass '{sessionTitle}'. Identified and preserved {bugCount} defects with immutable video evidence.\n\nKey Findings:\n{bugTitlesSummary}\n\nRecommended Action: Prioritize regression review on reported layout/transition failures.";
        }
    }

    public class GeminiVisionAiAssistant : IAiAssistantService
    {
        private readonly string _apiKey;
        private readonly string _modelName;
        private static readonly HttpClient _httpClient = new();

        public bool IsEnabled { get; set; }
        public string ProviderName => "Google Gemini Flash (Cloud Opt-in)";

        public GeminiVisionAiAssistant(string apiKey, string modelName = "gemini-2.5-flash")
        {
            _apiKey = apiKey;
            _modelName = modelName;
            IsEnabled = !string.IsNullOrWhiteSpace(apiKey);
        }

        public async Task<(string Title, string ObservedBehavior, string ReproSteps, string Suggestions)> AnalyzeClipAsync(string clipPath, double durationSeconds)
        {
            var fallback = new RuleBasedLocalAiAssistant();
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return await fallback.AnalyzeClipAsync(clipPath, durationSeconds);
            }

            try
            {
                // Inspect thumbnail image from clip if available
                var thumbPath = Path.ChangeExtension(clipPath, ".jpg");
                if (!File.Exists(thumbPath))
                {
                    return await fallback.AnalyzeClipAsync(clipPath, durationSeconds);
                }

                var imageBytes = await File.ReadAllBytesAsync(thumbPath);
                var base64Image = Convert.ToBase64String(imageBytes);

                var prompt = $"Analyze this captured screen evidence from a manual QA testing session. The clip captured the preceding {durationSeconds:0.0} seconds of user activity. Provide a structured response with: \n1) A concise Bug Title\n2) Observed Behavior description\n3) Candidate Reproduction Steps\n4) Technical suggestions/severity.";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = prompt },
                                new
                                {
                                    inline_data = new
                                    {
                                        mime_type = "image/jpeg",
                                        data = base64Image
                                    }
                                }
                            }
                        }
                    }
                };

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent?key={_apiKey}";
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    return await fallback.AnalyzeClipAsync(clipPath, durationSeconds);
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;
                var text = root.GetProperty("candidates")[0]
                               .GetProperty("content")
                               .GetProperty("parts")[0]
                               .GetProperty("text").GetString() ?? "";

                return ($"Screen Defect @ {DateTime.Now:HH:mm:ss}", text, "1. Reproduce workflow visible in captured video frame.", "Generated via Gemini Multimodal Vision from keyframe evidence.");
            }
            catch
            {
                return await fallback.AnalyzeClipAsync(clipPath, durationSeconds);
            }
        }

        public async Task<string> SummarizeSessionReportAsync(string sessionTitle, int bugCount, string bugTitlesSummary)
        {
            var fallback = new RuleBasedLocalAiAssistant();
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return await fallback.SummarizeSessionReportAsync(sessionTitle, bugCount, bugTitlesSummary);
            }

            try
            {
                var prompt = $"Summarize this QA testing session into an executive overview:\nSession: {sessionTitle}\nTotal Bugs: {bugCount}\nBugs:\n{bugTitlesSummary}";
                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    }
                };

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent?key={_apiKey}";
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);
                if (!response.IsSuccessStatusCode) return await fallback.SummarizeSessionReportAsync(sessionTitle, bugCount, bugTitlesSummary);

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("candidates")[0]
                                      .GetProperty("content")
                                      .GetProperty("parts")[0]
                                      .GetProperty("text").GetString() ?? await fallback.SummarizeSessionReportAsync(sessionTitle, bugCount, bugTitlesSummary);
            }
            catch
            {
                return await fallback.SummarizeSessionReportAsync(sessionTitle, bugCount, bugTitlesSummary);
            }
        }
    }
}
