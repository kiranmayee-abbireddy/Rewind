using System;
using System.Windows;
using System.Windows.Input;

namespace Rewind.App.Views
{
    public partial class FloatingHudWindow : Window
    {
        public event Action? OnPreserveRequested;
        public event Action? OnOpenMainRequested;

        public FloatingHudWindow()
        {
            InitializeComponent();
            PositionWindow();
        }

        private void HudCapsule_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    OnOpenMainRequested?.Invoke();
                }
                else
                {
                    try { DragMove(); } catch { }
                }
            }
        }

        private void PositionWindow()
        {
            var workingArea = SystemParameters.WorkArea;
            Left = workingArea.Right - Width - 24;
            Top = workingArea.Top + 24;
        }

        public void UpdateStatus(string state, double elapsedSeconds)
        {
            Dispatcher.Invoke(() =>
            {
                var span = TimeSpan.FromSeconds(elapsedSeconds);
                BufferTimeText.Text = $"{span.Minutes:00}:{span.Seconds:00}";

                if (state == "Recording")
                {
                    StatusDot.Background = (System.Windows.Media.Brush)FindResource("DangerBrush");
                    StatusText.Text = "REC";
                    StatusText.Foreground = (System.Windows.Media.Brush)FindResource("DangerBrush");
                }
                else if (state == "Paused")
                {
                    StatusDot.Background = (System.Windows.Media.Brush)FindResource("WarningBrush");
                    StatusText.Text = "PAUSE";
                    StatusText.Foreground = (System.Windows.Media.Brush)FindResource("WarningBrush");
                }
                else
                {
                    StatusDot.Background = (System.Windows.Media.Brush)FindResource("TextSecondaryBrush");
                    StatusText.Text = "IDLE";
                    StatusText.Foreground = (System.Windows.Media.Brush)FindResource("TextSecondaryBrush");
                }
            });
        }

        private void PreserveButton_Click(object sender, RoutedEventArgs e)
        {
            OnPreserveRequested?.Invoke();
        }

        private void OpenMain_Click(object sender, RoutedEventArgs e)
        {
            OnOpenMainRequested?.Invoke();
        }

        private void ToggleMinimize_Click(object sender, RoutedEventArgs e)
        {
            ExpandedContent.Visibility = Visibility.Collapsed;
        }

        private void LogoOrb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                // If minimized, clicking the logo orb restores the expanded controls
                if (ExpandedContent.Visibility == Visibility.Collapsed)
                {
                    ExpandedContent.Visibility = Visibility.Visible;
                    e.Handled = true;
                }
            }
        }
    }
}
