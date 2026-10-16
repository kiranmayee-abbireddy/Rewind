using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Rewind.App.Views
{
    public partial class TimelineEditorDialog : Window
    {
        private readonly double _currentElapsed;
        public string BugTitle { get; private set; } = string.Empty;
        public double ClipStartSeconds { get; private set; }
        public double ClipEndSeconds { get; private set; }
        public bool EnableAiDraft { get; private set; }

        public TimelineEditorDialog(double currentElapsedSeconds, string? thumbnailPath)
        {
            InitializeComponent();
            _currentElapsed = Math.Max(5, currentElapsedSeconds);

            // Set slider bounds
            StartSlider.Maximum = Math.Min(60, _currentElapsed);
            StartSlider.Value = Math.Min(30, StartSlider.Maximum);

            UpdateLabels();

            if (!string.IsNullOrEmpty(thumbnailPath) && File.Exists(thumbnailPath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(thumbnailPath, UriKind.Absolute);
                    bitmap.EndInit();
                    PreviewImage.Source = bitmap;
                }
                catch { }
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            if (RangeLabel == null || TimeBoundsLabel == null) return;

            var duration = StartSlider.Value;
            var start = Math.Max(0, _currentElapsed - duration);
            var end = _currentElapsed;

            RangeLabel.Text = $"Preserving last {duration:0.0}s of footage";
            TimeBoundsLabel.Text = $"{TimeSpan.FromSeconds(start):mm\\:ss} → {TimeSpan.FromSeconds(end):mm\\:ss}";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            BugTitle = string.IsNullOrWhiteSpace(BugTitleBox.Text) ? "Bug @" + DateTime.Now.ToString("HH:mm:ss") : BugTitleBox.Text.Trim();
            var duration = StartSlider.Value;
            ClipStartSeconds = Math.Max(0, _currentElapsed - duration);
            ClipEndSeconds = _currentElapsed;
            EnableAiDraft = AiAssistCheck.IsChecked ?? false;

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
