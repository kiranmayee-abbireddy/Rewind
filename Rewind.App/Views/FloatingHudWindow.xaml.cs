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
            MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    DragMove();
                }
            };
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
                    StatusDot.Fill = (System.Windows.Media.Brush)FindResource("DangerBrush");
                    StatusText.Text = "REC";
                    StatusText.Foreground = (System.Windows.Media.Brush)FindResource("DangerBrush");
                }
                else if (state == "Paused")
                {
                    StatusDot.Fill = (System.Windows.Media.Brush)FindResource("WarningBrush");
                    StatusText.Text = "PAUSE";
                    StatusText.Foreground = (System.Windows.Media.Brush)FindResource("WarningBrush");
                }
                else
                {
                    StatusDot.Fill = (System.Windows.Media.Brush)FindResource("TextSecondaryBrush");
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
    }
}
