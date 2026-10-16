using System.Windows;
using Rewind.App.Models;
using Rewind.App.Services;

namespace Rewind.App.Views
{
    public partial class SettingsDialog : Window
    {
        public AppSettings Settings { get; private set; }

        public SettingsDialog(AppSettings current)
        {
            InitializeComponent();
            Settings = current;

            BufferDurationSlider.Value = current.BufferDurationSeconds;
            BufferDurationValueLabel.Text = $"{current.BufferDurationSeconds}s";

            if (current.FrameRate == 30) FpsCombo.SelectedIndex = 1;
            else if (current.FrameRate == 60) FpsCombo.SelectedIndex = 2;
            else FpsCombo.SelectedIndex = 0;

            SourceCombo.SelectedIndex = current.CaptureSource.Contains("Window") || current.CaptureSource.Contains("Foreground") ? 1 : 0;

            AiEnabledCheck.IsChecked = current.AiEnabled;
            AudioEnabledCheck.IsChecked = current.RecordMicrophone;
            OllamaHostBox.Text = string.IsNullOrWhiteSpace(current.OllamaEndpoint) ? "http://localhost:11434" : current.OllamaEndpoint;

            var palette = string.IsNullOrWhiteSpace(current.ColorPalette) ? "Slate" : current.ColorPalette;
            ColorPaletteCombo.SelectedIndex = palette.ToLowerInvariant() switch
            {
                "emerald" => 1,
                "burgundy" => 2,
                "amber" => 3,
                "sapphire" => 4,
                _ => 0
            };
            DisplayModeCombo.SelectedIndex = current.IsDarkMode ? 0 : 1;

            var baseDir = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Rewind");
            StorageDirBox.Text = baseDir;
            CalculateStorageUsage(baseDir);

            Loaded += async (s, e) => await DetectAvailableModelsAsync();
        }

        private async void RefreshModels_Click(object sender, RoutedEventArgs e)
        {
            await DetectAvailableModelsAsync();
        }

        private async System.Threading.Tasks.Task DetectAvailableModelsAsync()
        {
            ModelStatusText.Text = "Detecting available local models via Ollama...";
            ModelStatusText.Foreground = (System.Windows.Media.Brush)FindResource("TextSecondaryBrush");

            var endpoint = string.IsNullOrWhiteSpace(OllamaHostBox.Text) ? "http://localhost:11434" : OllamaHostBox.Text.Trim().TrimEnd('/');
            var selectedModel = Settings.OllamaModel;

            try
            {
                using var client = new System.Net.Http.HttpClient { Timeout = System.TimeSpan.FromSeconds(3) };
                var response = await client.GetAsync($"{endpoint}/api/tags");
                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    using var doc = System.Text.Json.JsonDocument.Parse(jsonStr);
                    if (doc.RootElement.TryGetProperty("models", out var modelsProp) && modelsProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        var modelList = new System.Collections.Generic.List<string>();
                        foreach (var m in modelsProp.EnumerateArray())
                        {
                            if (m.TryGetProperty("name", out var nameProp))
                            {
                                var mName = nameProp.GetString();
                                if (!string.IsNullOrWhiteSpace(mName)) modelList.Add(mName);
                            }
                        }

                        if (modelList.Count > 0)
                        {
                            OllamaModelCombo.Items.Clear();
                            int selectIdx = 0;
                            for (int i = 0; i < modelList.Count; i++)
                            {
                                var m = modelList[i];
                                OllamaModelCombo.Items.Add(m);
                                if (m.Equals(selectedModel, System.StringComparison.OrdinalIgnoreCase)) selectIdx = i;
                            }
                            OllamaModelCombo.SelectedIndex = selectIdx;
                            ModelStatusText.Text = $"✔ Found {modelList.Count} model(s) installed on this PC.";
                            ModelStatusText.Foreground = (System.Windows.Media.Brush)FindResource("SuccessBrush");
                            return;
                        }
                    }
                }
            }
            catch { }

            // Fallback list if Ollama is not yet started or offline
            if (OllamaModelCombo.Items.Count == 0)
            {
                OllamaModelCombo.Items.Add("llama3:latest");
                OllamaModelCombo.Items.Add("gemma3:1b");
                OllamaModelCombo.Items.Add("qwen3:4b");

                int idx = 0;
                for (int i = 0; i < OllamaModelCombo.Items.Count; i++)
                {
                    if (OllamaModelCombo.Items[i].ToString()!.Equals(selectedModel, System.StringComparison.OrdinalIgnoreCase)) idx = i;
                }
                OllamaModelCombo.SelectedIndex = idx;
            }
            ModelStatusText.Text = "Ollama service offline or unreachable. Using default model options.";
            ModelStatusText.Foreground = (System.Windows.Media.Brush)FindResource("WarningBrush");
        }

        private void CalculateStorageUsage(string dir)
        {
            try
            {
                if (System.IO.Directory.Exists(dir))
                {
                    long totalBytes = 0;
                    var files = System.IO.Directory.GetFiles(dir, "*.*", System.IO.SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        totalBytes += new System.IO.FileInfo(file).Length;
                    }
                    var mb = totalBytes / (1024.0 * 1024.0);
                    StorageUsageLabel.Text = $"Total disk footprint: {mb:0.0} MB used across {files.Length} clips/metadata files.";
                    StorageProgressBar.Value = System.Math.Min(100, (int)(mb / 5.0)); // proportional to 500MB target
                }
                else
                {
                    StorageUsageLabel.Text = "Storage directory is ready (0 MB).";
                    StorageProgressBar.Value = 0;
                }
            }
            catch
            {
                StorageUsageLabel.Text = "Storage managed locally.";
            }
        }

        private void OpenStorageFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dir = StorageDirBox.Text;
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private void BufferDurationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (BufferDurationValueLabel != null)
            {
                BufferDurationValueLabel.Text = $"{e.NewValue:0}s";
            }
        }

        private void PurgeBuffer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bufferDir = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Rewind", "buffer");
                if (System.IO.Directory.Exists(bufferDir))
                {
                    var files = System.IO.Directory.GetFiles(bufferDir);
                    foreach (var f in files)
                    {
                        try { System.IO.File.Delete(f); } catch { }
                    }
                }
                MessageBox.Show("Rolling buffer temporary files purged successfully.", "Buffer Cleaned", MessageBoxButton.OK, MessageBoxImage.Information);
                CalculateStorageUsage(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Rewind"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to purge buffer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ThemeSelection_Changed(object sender, RoutedEventArgs e)
        {
            if (ColorPaletteCombo == null || DisplayModeCombo == null) return;
            var selectedPalette = ColorPaletteCombo.SelectedIndex switch
            {
                1 => "Emerald",
                2 => "Burgundy",
                3 => "Amber",
                4 => "Sapphire",
                _ => "Slate"
            };
            var isDark = DisplayModeCombo.SelectedIndex == 0;
            ThemeManager.ApplyTheme(selectedPalette, isDark);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Settings.BufferDurationSeconds = (int)BufferDurationSlider.Value;
            Settings.FrameRate = FpsCombo.SelectedIndex switch
            {
                1 => 30,
                2 => 60,
                _ => 15
            };
            Settings.CaptureSource = SourceCombo.SelectedIndex == 1 ? "Active Foreground Window" : "Primary Display (Entire Screen)";
            Settings.AiEnabled = AiEnabledCheck.IsChecked ?? true;
            Settings.RecordMicrophone = AudioEnabledCheck.IsChecked ?? false;
            
            var selectedPalette = ColorPaletteCombo.SelectedIndex switch
            {
                1 => "Emerald",
                2 => "Burgundy",
                3 => "Amber",
                4 => "Sapphire",
                _ => "Slate"
            };
            var isDark = DisplayModeCombo.SelectedIndex == 0;

            Settings.ColorPalette = selectedPalette;
            Settings.IsDarkMode = isDark;
            Settings.ThemeMode = $"{selectedPalette}_{(isDark ? "Dark" : "Light")}";

            Settings.OllamaEndpoint = string.IsNullOrWhiteSpace(OllamaHostBox.Text) ? "http://localhost:11434" : OllamaHostBox.Text.Trim();
            Settings.OllamaModel = OllamaModelCombo.SelectedItem?.ToString() ?? (string.IsNullOrWhiteSpace(OllamaModelCombo.Text) ? "llama3:latest" : OllamaModelCombo.Text);

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
