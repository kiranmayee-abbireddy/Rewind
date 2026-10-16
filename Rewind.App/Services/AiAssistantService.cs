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
        Task<(string Title, string ObservedBehavior, string ReproSteps, string Suggestions)> AnalyzeClipAsync(string clipPath, double durationSeconds, string userCaseOrContext = "");
        Task<string> SummarizeSessionReportAsync(string sessionTitle, int bugCount, string bugTitlesSummary);
    }

    public class OllamaAiAssistant : IAiAssistantService
    {
        private readonly string _endpoint;
        private readonly string _modelName;
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

        public bool IsEnabled { get; set; } = true;
        public string ProviderName => $"Ollama ({_modelName})";

        public OllamaAiAssistant(string endpoint = "http://localhost:11434", string modelName = "qwen3:4b")
        {
            _endpoint = endpoint.TrimEnd('/');
            _modelName = modelName;
        }

        public async Task<(string Title, string ObservedBehavior, string ReproSteps, string Suggestions)> AnalyzeClipAsync(string clipPath, double durationSeconds, string userCaseOrContext = "")
        {
            var fallback = new RuleBasedLocalAiAssistant();
            try
            {
                var prompt = $@"You are an expert QA Software Test Engineer.
A tester captured a {durationSeconds:0.0}s bug evidence video clip.
User's notes / test case: ""{userCaseOrContext}""

Generate a clear, professional defect report in concise format:
TITLE: [Concise, high-impact bug title describing the core failure, incorporating the user's case]
OBSERVED: [Clear description of what went wrong]
REPRO: [Numbered 1-4 step reproduction sequence]
SUGGESTIONS: [Technical severity rating & suggestions]

Do not include conversational preamble. Respond directly with TITLE:, OBSERVED:, REPRO:, and SUGGESTIONS:.";

                var requestObj = new
                {
                    model = _modelName,
                    prompt = prompt,
                    stream = false
                };

                var content = new StringContent(JsonSerializer.Serialize(requestObj), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_endpoint}/api/generate", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonStr);
                    if (doc.RootElement.TryGetProperty("response", out var respProp))
                    {
                        var text = respProp.GetString() ?? string.Empty;
                        return ParseLlmResponse(text, userCaseOrContext, durationSeconds);
                    }
                }
            }
            catch { }

            return await fallback.AnalyzeClipAsync(clipPath, durationSeconds, userCaseOrContext);
        }

        private static (string Title, string ObservedBehavior, string ReproSteps, string Suggestions) ParseLlmResponse(string text, string userContext, double duration)
        {
            string title = string.Empty;
            string observed = string.Empty;
            string repro = string.Empty;
            string suggestions = string.Empty;

            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string currentSection = "TITLE";

            var sbObserved = new StringBuilder();
            var sbRepro = new StringBuilder();
            var sbSuggestions = new StringBuilder();

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line.StartsWith("TITLE:", StringComparison.OrdinalIgnoreCase))
                {
                    title = line.Substring("TITLE:".Length).Trim();
                    currentSection = "TITLE";
                }
                else if (line.StartsWith("OBSERVED:", StringComparison.OrdinalIgnoreCase) || line.StartsWith("OBSERVED BEHAVIOR:", StringComparison.OrdinalIgnoreCase))
                {
                    sbObserved.AppendLine(line.Substring(line.IndexOf(':') + 1).Trim());
                    currentSection = "OBSERVED";
                }
                else if (line.StartsWith("REPRO:", StringComparison.OrdinalIgnoreCase) || line.StartsWith("REPRODUCTION STEPS:", StringComparison.OrdinalIgnoreCase))
                {
                    sbRepro.AppendLine(line.Substring(line.IndexOf(':') + 1).Trim());
                    currentSection = "REPRO";
                }
                else if (line.StartsWith("SUGGESTIONS:", StringComparison.OrdinalIgnoreCase))
                {
                    sbSuggestions.AppendLine(line.Substring("SUGGESTIONS:".Length).Trim());
                    currentSection = "SUGGESTIONS";
                }
                else
                {
                    if (currentSection == "OBSERVED") sbObserved.AppendLine(line);
                    else if (currentSection == "REPRO") sbRepro.AppendLine(line);
                    else if (currentSection == "SUGGESTIONS") sbSuggestions.AppendLine(line);
                    else if (string.IsNullOrEmpty(title)) title = line;
                }
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title = !string.IsNullOrWhiteSpace(userContext) ? $"Defect in {userContext}" : $"Defect captured in {duration:0.0}s clip";
            }

            if (sbObserved.Length == 0) sbObserved.AppendLine($"Anomalous behavior observed during testing of '{userContext}'.");
            if (sbRepro.Length == 0) sbRepro.AppendLine($"1. Navigate to target view.\n2. Execute workflow: {userContext}.\n3. Observe defect.");
            if (sbSuggestions.Length == 0) sbSuggestions.AppendLine("Severity: Medium. Review associated logs and network payload.");

            return (title.Trim(), sbObserved.ToString().Trim(), sbRepro.ToString().Trim(), sbSuggestions.ToString().Trim());
        }

        public async Task<string> SummarizeSessionReportAsync(string sessionTitle, int bugCount, string bugTitlesSummary)
        {
            try
            {
                var prompt = $@"Summarize this testing session as a QA Executive Summary:
Session: {sessionTitle}
Defects Preserved: {bugCount}
Defect List:
{bugTitlesSummary}

Write a professional executive summary covering key risks, defect clusters, and sign-off recommendations.";

                var requestObj = new
                {
                    model = _modelName,
                    prompt = prompt,
                    stream = false
                };

                var content = new StringContent(JsonSerializer.Serialize(requestObj), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_endpoint}/api/generate", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonStr);
                    if (doc.RootElement.TryGetProperty("response", out var respProp))
                    {
                        var text = respProp.GetString();
                        if (!string.IsNullOrWhiteSpace(text)) return text.Trim();
                    }
                }
            }
            catch { }

            var fallback = new RuleBasedLocalAiAssistant();
            return await fallback.SummarizeSessionReportAsync(sessionTitle, bugCount, bugTitlesSummary);
        }
    }

    public class RuleBasedLocalAiAssistant : IAiAssistantService
    {
        public bool IsEnabled { get; set; } = true;
        public string ProviderName => "REWIND Local Rule Engine (Offline)";

        public async Task<(string Title, string ObservedBehavior, string ReproSteps, string Suggestions)> AnalyzeClipAsync(string clipPath, double durationSeconds, string userCaseOrContext = "")
        {
            await Task.Delay(100);

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var title = !string.IsNullOrWhiteSpace(userCaseOrContext)
                ? $"Defect in [{userCaseOrContext.Trim()}] @ {timestamp}"
                : $"Unexpected UI state transition detected @ {timestamp}";

            var observed = $"Anomalous interaction recorded during '{(!string.IsNullOrWhiteSpace(userCaseOrContext) ? userCaseOrContext : "workflow")}'. Screen visual activity captured ({durationSeconds:0.0}s).";
            var repro = $"1. Launch application under test.\n2. Navigate to screen active during discovery.\n3. Execute scenario: {(!string.IsNullOrWhiteSpace(userCaseOrContext) ? userCaseOrContext : "reproduction sequence")}.\n4. Observe discrepancy with expected state.";
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

        public async Task<(string Title, string ObservedBehavior, string ReproSteps, string Suggestions)> AnalyzeClipAsync(string clipPath, double durationSeconds, string userCaseOrContext = "")
        {
            var fallback = new RuleBasedLocalAiAssistant();
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return await fallback.AnalyzeClipAsync(clipPath, durationSeconds, userCaseOrContext);
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

                var prompt = $"Analyze this captured screen evidence from a manual QA testing session. The clip captured the preceding {durationSeconds:0.0} seconds of user activity. Tester context/case: \"{userCaseOrContext}\". Provide a structured response with: \n1) A concise Bug Title (incorporating tester context)\n2) Observed Behavior description\n3) Candidate Reproduction Steps\n4) Technical suggestions/severity.";

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
