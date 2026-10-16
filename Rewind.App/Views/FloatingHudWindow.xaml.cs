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

        private void MascotOrb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                // Dragging or clicking
                if (e.ClickCount == 2)
                {
                    OnOpenMainRequested?.Invoke();
                }
                else
                {
                    // Toggle Action Deck visibility on single click, or drag
                    ActionDeck.Visibility = ActionDeck.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
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
    }
}
