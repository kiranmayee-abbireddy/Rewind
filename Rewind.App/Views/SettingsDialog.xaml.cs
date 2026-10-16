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

            AiEnabledCheck.IsChecked = current.AiEnabled;
            AudioEnabledCheck.IsChecked = current.RecordMicrophone;

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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Settings.BufferDurationSeconds = (int)BufferDurationSlider.Value;
            Settings.FrameRate = FpsCombo.SelectedIndex switch
            {
                1 => 30,
                2 => 60,
                _ => 15
            };
            Settings.AiEnabled = AiEnabledCheck.IsChecked ?? true;
            Settings.RecordMicrophone = AudioEnabledCheck.IsChecked ?? false;

            DialogResult = true;
            Close();
        }
    }
}
