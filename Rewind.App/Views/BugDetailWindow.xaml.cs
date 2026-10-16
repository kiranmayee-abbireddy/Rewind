using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Rewind.App.Models;

namespace Rewind.App.Views
{
    public partial class BugDetailWindow : Window
    {
        private readonly Bug _bug;
        public bool IsDeleted { get; private set; }

        public BugDetailWindow(Bug bug)
        {
            InitializeComponent();
            _bug = bug;

            TitleBox.Text = bug.Title;
            MetaInfoText.Text = $"Captured at {bug.CreatedAt:HH:mm:ss} • {bug.DurationSeconds:0.0}s clip ({bug.ClipStartSeconds:0.0}s → {bug.ClipEndSeconds:0.0}s)";
            ClipPathLabel.Text = $"File: {bug.ClipPath}";
            ObservedBox.Text = bug.ObservedBehavior;
            ExpectedBox.Text = bug.ExpectedBehavior;

            if (bug.AiAssisted && !string.IsNullOrWhiteSpace(bug.AiSuggestions))
            {
                AiContainer.Visibility = Visibility.Visible;
                AiNotesBlock.Text = bug.AiSuggestions;
            }
            else
            {
                AiContainer.Visibility = Visibility.Collapsed;
            }

            // Load thumbnail
            var thumb = !string.IsNullOrEmpty(bug.ThumbnailPath) && File.Exists(bug.ThumbnailPath)
                ? bug.ThumbnailPath
                : Path.ChangeExtension(bug.ClipPath, ".jpg");

            if (File.Exists(thumb))
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(thumb, UriKind.Absolute);
                    bmp.EndInit();
                    ThumbnailImage.Source = bmp;
                }
                catch { }
            }

            if (File.Exists(bug.ClipPath))
            {
                try
                {
                    ClipMediaPlayer.LoadedBehavior = MediaState.Manual;
                    ClipMediaPlayer.UnloadedBehavior = MediaState.Manual;
                    ClipMediaPlayer.Source = new Uri(bug.ClipPath, UriKind.Absolute);

                    ClipMediaPlayer.MediaOpened += (s, e) =>
                    {
                        if (ClipMediaPlayer.NaturalDuration.HasTimeSpan)
                        {
                            MediaPositionSlider.Maximum = ClipMediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                            UpdateMediaTimeLabel(TimeSpan.Zero, ClipMediaPlayer.NaturalDuration.TimeSpan);
                        }
                    };

                    ClipMediaPlayer.MediaEnded += (s, e) =>
                    {
                        ClipMediaPlayer.Stop();
                        _mediaTimer?.Stop();
                        _isPlaying = false;
                        PlayPauseMediaBtn.Content = "▶ Play";
                        MediaPositionSlider.Value = 0;
                        ThumbnailImage.Visibility = Visibility.Visible;
                    };

                    ClipMediaPlayer.MediaFailed += (s, e) =>
                    {
                        _mediaTimer?.Stop();
                        _isPlaying = false;
                        PlayPauseMediaBtn.Content = "▶ Play";
                        ThumbnailImage.Visibility = Visibility.Visible;
                        ClipPathLabel.Text = $"Video playback error: {e.ErrorException?.Message}. Click 'Play Video in Player' above.";
                    };

                    _mediaTimer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(200)
                    };
                    _mediaTimer.Tick += (s, e) =>
                    {
                        if (!_isUserDraggingSlider && ClipMediaPlayer.NaturalDuration.HasTimeSpan)
                        {
                            MediaPositionSlider.Value = ClipMediaPlayer.Position.TotalSeconds;
                            UpdateMediaTimeLabel(ClipMediaPlayer.Position, ClipMediaPlayer.NaturalDuration.TimeSpan);
                        }
                    };
                }
                catch { }
            }

            Closed += (s, e) =>
            {
                _mediaTimer?.Stop();
                ClipMediaPlayer.Close();
            };

            MediaPositionSlider.PreviewMouseDown += (s, e) => _isUserDraggingSlider = true;
            MediaPositionSlider.PreviewMouseUp += (s, e) =>
            {
                _isUserDraggingSlider = false;
                if (ClipMediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    ClipMediaPlayer.Position = TimeSpan.FromSeconds(MediaPositionSlider.Value);
                }
            };
        }

        private System.Windows.Threading.DispatcherTimer? _mediaTimer;
        private bool _isPlaying;
        private bool _isUserDraggingSlider;

        private void TogglePlayMedia_Click(object sender, RoutedEventArgs e)
        {
            if (_isPlaying)
            {
                ClipMediaPlayer.Pause();
                _mediaTimer?.Stop();
                _isPlaying = false;
                PlayPauseMediaBtn.Content = "▶ Play";
            }
            else
            {
                ThumbnailImage.Visibility = Visibility.Collapsed;
                ClipMediaPlayer.Play();
                _mediaTimer?.Start();
                _isPlaying = true;
                PlayPauseMediaBtn.Content = "⏸ Pause";
            }
        }

        private void MediaPositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ClipMediaPlayer.NaturalDuration.HasTimeSpan)
            {
                var target = TimeSpan.FromSeconds(MediaPositionSlider.Value);
                if (Math.Abs((ClipMediaPlayer.Position - target).TotalSeconds) > 0.5)
                {
                    ClipMediaPlayer.Position = target;
                }
                UpdateMediaTimeLabel(target, ClipMediaPlayer.NaturalDuration.TimeSpan);
            }
        }

        private void UpdateMediaTimeLabel(TimeSpan current, TimeSpan total)
        {
            MediaTimeLabel.Text = $"{current:mm\\:ss} / {total:mm\\:ss}";
        }

        private void PlayVideo_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(_bug.ClipPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _bug.ClipPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not launch media player: {ex.Message}", "Playback Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Clip video file was not found on disk.", "File Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show($"Are you sure you want to permanently delete bug '{_bug.Title}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                IsDeleted = true;
                DialogResult = true;
                Close();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _bug.Title = string.IsNullOrWhiteSpace(TitleBox.Text) ? _bug.Title : TitleBox.Text.Trim();
            _bug.ObservedBehavior = ObservedBox.Text.Trim();
            _bug.ExpectedBehavior = ExpectedBox.Text.Trim();

            DialogResult = true;
            Close();
        }
    }
}
