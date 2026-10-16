using System.Windows;
using Rewind.App.Models;

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

            if (current.OllamaModel == "gemma3:1b") OllamaModelCombo.SelectedIndex = 1;
            else if (current.OllamaModel == "qwen3:4b") OllamaModelCombo.SelectedIndex = 2;
            else OllamaModelCombo.SelectedIndex = 0;

            var baseDir = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Rewind");
            StorageDirBox.Text = baseDir;
            CalculateStorageUsage(baseDir);
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
            Settings.OllamaEndpoint = string.IsNullOrWhiteSpace(OllamaHostBox.Text) ? "http://localhost:11434" : OllamaHostBox.Text.Trim();
            Settings.OllamaModel = OllamaModelCombo.SelectedIndex switch
            {
                1 => "gemma3:1b",
                2 => "qwen3:4b",
                _ => "llama3:latest"
            };

            DialogResult = true;
            Close();
        }
    }
}
