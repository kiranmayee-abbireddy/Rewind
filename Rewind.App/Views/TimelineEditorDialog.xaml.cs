using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Rewind.App.Views
{
    public partial class TimelineEditorDialog : Window
    {
        private readonly double _currentElapsed;
        private readonly Func<double, string?>? _frameLookup;
        public string BugTitle { get; private set; } = string.Empty;
        public double ClipStartSeconds { get; private set; }
        public double ClipEndSeconds { get; private set; }
        public bool EnableAiDraft { get; private set; }
        public System.Collections.Generic.List<Rect> NormalizedRedactions { get; } = new();

        private Point _startPoint;
        private System.Windows.Shapes.Rectangle? _currentRect;
        private bool _isDrawingRedaction;

        private System.Windows.Threading.DispatcherTimer? _previewTimer;
        private bool _isPreviewPlaying;
        private double _previewPlayhead;

        public TimelineEditorDialog(double currentElapsedSeconds, string? thumbnailPath, Func<double, string?>? frameLookup = null)
        {
            InitializeComponent();
            _currentElapsed = Math.Max(1.0, currentElapsedSeconds);
            _frameLookup = frameLookup;

            var minTime = Math.Max(0, _currentElapsed - 60.0);
            var maxTime = _currentElapsed;

            StartSlider.Minimum = minTime;
            StartSlider.Maximum = maxTime;
            EndSlider.Minimum = minTime;
            EndSlider.Maximum = maxTime;

            // Default to past 30s or available window
            StartSlider.Value = Math.Max(minTime, maxTime - 30.0);
            EndSlider.Value = maxTime;

            UpdateLabels();
            ShowFrame(thumbnailPath);

            _previewTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _previewTimer.Tick += PreviewTimer_Tick;

            Closed += (s, e) =>
            {
                _previewTimer?.Stop();
            };
        }

        private void ShowFrame(string? path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(path, UriKind.Absolute);
                    bitmap.EndInit();
                    PreviewImage.Source = bitmap;
                }
                catch { }
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (StartSlider == null || EndSlider == null) return;

            // Ensure start does not exceed end
            if (sender == StartSlider && StartSlider.Value > EndSlider.Value)
            {
                EndSlider.Value = Math.Min(EndSlider.Maximum, StartSlider.Value + 1.0);
            }
            else if (sender == EndSlider && EndSlider.Value < StartSlider.Value)
            {
                StartSlider.Value = Math.Max(StartSlider.Minimum, EndSlider.Value - 1.0);
            }

            UpdateLabels();

            // Live scrub image based on adjusted slider
            if (_frameLookup != null)
            {
                var targetTime = (sender == StartSlider) ? StartSlider.Value : EndSlider.Value;
                var framePath = _frameLookup(targetTime);
                ShowFrame(framePath);
            }
        }

        private void PlayPreview_Click(object sender, RoutedEventArgs e)
        {
            if (_isPreviewPlaying)
            {
                _previewTimer?.Stop();
                _isPreviewPlaying = false;
                PlayPreviewBtn.Content = "▶ Play";
            }
            else
            {
                _previewPlayhead = StartSlider.Value;
                _previewTimer?.Start();
                _isPreviewPlaying = true;
                PlayPreviewBtn.Content = "⏸ Pause";
            }
        }

        private void PreviewTimer_Tick(object? sender, EventArgs e)
        {
            _previewPlayhead += 0.1;
            if (_previewPlayhead > EndSlider.Value)
            {
                _previewPlayhead = StartSlider.Value;
            }

            if (_frameLookup != null)
            {
                var framePath = _frameLookup(_previewPlayhead);
                ShowFrame(framePath);
            }
        }

        private void Preset15_Click(object sender, RoutedEventArgs e)
        {
            EndSlider.Value = _currentElapsed;
            StartSlider.Value = Math.Max(StartSlider.Minimum, _currentElapsed - 15.0);
        }

        private void Preset30_Click(object sender, RoutedEventArgs e)
        {
            EndSlider.Value = _currentElapsed;
            StartSlider.Value = Math.Max(StartSlider.Minimum, _currentElapsed - 30.0);
        }

        private void Preset60_Click(object sender, RoutedEventArgs e)
        {
            EndSlider.Value = _currentElapsed;
            StartSlider.Value = Math.Max(StartSlider.Minimum, _currentElapsed - 60.0);
        }

        private void UpdateLabels()
        {
            if (RangeLabel == null || TimeBoundsLabel == null || StartValueLabel == null || EndValueLabel == null) return;

            var start = StartSlider.Value;
            var end = EndSlider.Value;
            var duration = Math.Max(0.1, end - start);

            StartValueLabel.Text = $"{TimeSpan.FromSeconds(start):mm\\:ss}";
            EndValueLabel.Text = $"{TimeSpan.FromSeconds(end):mm\\:ss}";

            RangeLabel.Text = $"Preserving {duration:0.0}s clip";
            TimeBoundsLabel.Text = $"{TimeSpan.FromSeconds(start):mm\\:ss} → {TimeSpan.FromSeconds(end):mm\\:ss}";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _previewTimer?.Stop();
            BugTitle = string.IsNullOrWhiteSpace(BugTitleBox.Text) ? "Bug @" + DateTime.Now.ToString("HH:mm:ss") : BugTitleBox.Text.Trim();
            ClipStartSeconds = StartSlider.Value;
            ClipEndSeconds = EndSlider.Value;
            EnableAiDraft = AiAssistCheck.IsChecked ?? false;

            DialogResult = true;
            Close();
        }

        private void RedactionCanvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                _isDrawingRedaction = true;
                _startPoint = e.GetPosition(RedactionCanvas);

                _currentRect = new System.Windows.Shapes.Rectangle
                {
                    Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(220, 0, 0, 0)),
                    Stroke = System.Windows.Media.Brushes.Yellow,
                    StrokeThickness = 1.5,
                    StrokeDashArray = new System.Windows.Media.DoubleCollection { 2, 2 }
                };

                System.Windows.Controls.Canvas.SetLeft(_currentRect, _startPoint.X);
                System.Windows.Controls.Canvas.SetTop(_currentRect, _startPoint.Y);
                RedactionCanvas.Children.Add(_currentRect);
            }
        }

        private void RedactionCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isDrawingRedaction && _currentRect != null)
            {
                var cur = e.GetPosition(RedactionCanvas);
                var x = Math.Min(cur.X, _startPoint.X);
                var y = Math.Min(cur.Y, _startPoint.Y);
                var w = Math.Abs(cur.X - _startPoint.X);
                var h = Math.Abs(cur.Y - _startPoint.Y);

                System.Windows.Controls.Canvas.SetLeft(_currentRect, x);
                System.Windows.Controls.Canvas.SetTop(_currentRect, y);
                _currentRect.Width = w;
                _currentRect.Height = h;
            }
        }

        private void RedactionCanvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isDrawingRedaction && _currentRect != null)
            {
                _isDrawingRedaction = false;
                if (_currentRect.Width > 5 && _currentRect.Height > 5 && RedactionCanvas.ActualWidth > 0 && RedactionCanvas.ActualHeight > 0)
                {
                    var x = System.Windows.Controls.Canvas.GetLeft(_currentRect);
                    var y = System.Windows.Controls.Canvas.GetTop(_currentRect);
                    NormalizedRedactions.Add(new Rect(
                        x / RedactionCanvas.ActualWidth,
                        y / RedactionCanvas.ActualHeight,
                        _currentRect.Width / RedactionCanvas.ActualWidth,
                        _currentRect.Height / RedactionCanvas.ActualHeight
                    ));
                }
            }
        }

        private void ClearRedactions_Click(object sender, RoutedEventArgs e)
        {
            RedactionCanvas.Children.Clear();
            NormalizedRedactions.Clear();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
