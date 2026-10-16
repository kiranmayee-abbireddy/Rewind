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
                var timeStr = $"{span.Minutes:00}:{span.Seconds:00}";
                BufferTimeText.Text = timeStr;
                MiniBufferTimeText.Text = timeStr;

                System.Windows.Media.Brush statusBrush;
                string statusLabel;

                if (state == "Recording")
                {
                    statusBrush = (System.Windows.Media.Brush)FindResource("DangerBrush");
                    statusLabel = "REC";
                }
                else if (state == "Paused")
                {
                    statusBrush = (System.Windows.Media.Brush)FindResource("WarningBrush");
                    statusLabel = "PAUSE";
                }
                else
                {
                    statusBrush = (System.Windows.Media.Brush)FindResource("TextSecondaryBrush");
                    statusLabel = "IDLE";
                }

                StatusDot.Background = statusBrush;
                StatusText.Text = statusLabel;
                StatusText.Foreground = statusBrush;

                MiniStatusDot.Background = statusBrush;
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
            HudCapsule.Visibility = Visibility.Collapsed;
            MinimizedCapsule.Visibility = Visibility.Visible;
        }

        private void MinimizedCapsule_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    OnOpenMainRequested?.Invoke();
                }
                else
                {
                    // Single click restores the full HUD capsule
                    MinimizedCapsule.Visibility = Visibility.Collapsed;
                    HudCapsule.Visibility = Visibility.Visible;
                    e.Handled = true;
                }
            }
        }

        private void MiniPreserve_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                e.Handled = true;
                OnPreserveRequested?.Invoke();
            }
        }
    }
}
