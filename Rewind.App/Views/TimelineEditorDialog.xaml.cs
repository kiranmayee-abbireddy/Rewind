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
        public System.Collections.Generic.List<Rect> NormalizedRedactions { get; } = new();

        private Point _startPoint;
        private System.Windows.Shapes.Rectangle? _currentRect;
        private bool _isDrawingRedaction;

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

        private void Preset15_Click(object sender, RoutedEventArgs e)
        {
            StartSlider.Value = Math.Min(15, StartSlider.Maximum);
        }

        private void Preset30_Click(object sender, RoutedEventArgs e)
        {
            StartSlider.Value = Math.Min(30, StartSlider.Maximum);
        }

        private void Preset60_Click(object sender, RoutedEventArgs e)
        {
            StartSlider.Value = Math.Min(60, StartSlider.Maximum);
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
