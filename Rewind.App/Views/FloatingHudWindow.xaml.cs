using System;
using System.Windows;
using System.Windows.Input;

namespace Rewind.App.Views
{
    public partial class FloatingHudWindow : Window
    {
        public event Action? OnPreserveRequested;
        public event Action? OnOpenMainRequested;
        public event Action<bool>? OnVisibilityChanged;

        private const double SnapDistance = 35.0;
        private const double EdgeMargin = 8.0;

        public FloatingHudWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => PositionWindow();
            IsVisibleChanged += (s, e) => OnVisibilityChanged?.Invoke(IsVisible);
        }

        public void ShowAndExpand()
        {
            Show();
            Activate();
            ExpandCapsule();
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
                    try
                    {
                        DragMove();
                        SnapToNearestEdge();
                    }
                    catch { }
                }
            }
        }

        private void PositionWindow()
        {
            var workingArea = SystemParameters.WorkArea;
            Left = workingArea.Right - ActualWidth - SnapDistance;
            Top = workingArea.Top + 40;
            SnapToNearestEdge();
        }

        private void SnapToNearestEdge()
        {
            var workingArea = SystemParameters.WorkArea;
            var w = ActualWidth > 0 ? ActualWidth : Width;
            var h = ActualHeight > 0 ? ActualHeight : Height;
            if (double.IsNaN(w) || w <= 0) w = 240;
            if (double.IsNaN(h) || h <= 0) h = 60;

            // Clamping Top within workspace bounds
            if (Top < workingArea.Top + EdgeMargin) Top = workingArea.Top + EdgeMargin;
            if (Top + h > workingArea.Bottom - EdgeMargin) Top = workingArea.Bottom - h - EdgeMargin;

            // Magnetic snap to Left, Right, Top, or Bottom
            var distLeft = Math.Abs(Left - workingArea.Left);
            var distRight = Math.Abs((workingArea.Right - w) - Left);
            var distTop = Math.Abs(Top - workingArea.Top);
            var distBottom = Math.Abs((workingArea.Bottom - h) - Top);

            // Horizontal Snap (Left/Right dock -> Vertical bar orientation)
            bool isVerticalEdge = false;
            if (distLeft < SnapDistance)
            {
                Left = workingArea.Left + EdgeMargin;
                isVerticalEdge = true;
            }
            else if (distRight < SnapDistance)
            {
                Left = workingArea.Right - w - EdgeMargin;
                isVerticalEdge = true;
            }

            // Vertical Snap (Top/Bottom dock -> Horizontal bar orientation)
            if (distTop < SnapDistance)
            {
                Top = workingArea.Top + EdgeMargin;
                isVerticalEdge = false;
            }
            else if (distBottom < SnapDistance)
            {
                Top = workingArea.Bottom - h - EdgeMargin;
                isVerticalEdge = false;
            }

            // Ensure window never escapes the screen boundary
            if (Left < workingArea.Left) Left = workingArea.Left + EdgeMargin;
            if (Left + w > workingArea.Right) Left = workingArea.Right - w - EdgeMargin;
        }

        public void UpdateStatus(string state, double elapsedSeconds)
        {
            Dispatcher.Invoke(() =>
            {
                var span = TimeSpan.FromSeconds(elapsedSeconds);
                var timeStr = $"{span.Minutes:00}:{span.Seconds:00}";
                BufferTimeText.Text = timeStr;

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

        private void HideHud_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void ToggleMinimize_Click(object sender, RoutedEventArgs e)
        {
            HudCapsule.Visibility = Visibility.Collapsed;
            MinimizedCapsule.Visibility = Visibility.Visible;
            Dispatcher.InvokeAsync(SnapToNearestEdge, System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private Point _miniMouseDownScreenPos;
        private bool _isMiniMouseDown;
        private bool _isMiniDragging;

        private void MinimizedCapsule_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _miniMouseDownScreenPos = PointToScreen(e.GetPosition(this));
            _isMiniMouseDown = true;
            _isMiniDragging = false;

            if (e.ClickCount == 2)
            {
                OnOpenMainRequested?.Invoke();
                _isMiniMouseDown = false;
                e.Handled = true;
            }
        }

        private void MinimizedCapsule_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMiniMouseDown && e.LeftButton == MouseButtonState.Pressed && !_isMiniDragging)
            {
                var currentScreenPos = PointToScreen(e.GetPosition(this));
                var dx = Math.Abs(currentScreenPos.X - _miniMouseDownScreenPos.X);
                var dy = Math.Abs(currentScreenPos.Y - _miniMouseDownScreenPos.Y);

                if (dx > SystemParameters.MinimumHorizontalDragDistance || dy > SystemParameters.MinimumVerticalDragDistance)
                {
                    _isMiniDragging = true;
                    try
                    {
                        DragMove();
                        SnapToNearestEdge();
                    }
                    catch { }
                    _isMiniMouseDown = false;
                }
            }
        }

        private void MinimizedCapsule_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isMiniMouseDown && !_isMiniDragging)
            {
                ExpandCapsule();
                e.Handled = true;
            }
            _isMiniMouseDown = false;
            _isMiniDragging = false;
        }

        private void MinimizedCapsule_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Hide();
            e.Handled = true;
        }

        private void ExpandCapsule()
        {
            MinimizedCapsule.Visibility = Visibility.Collapsed;
            HudCapsule.Visibility = Visibility.Visible;
            Dispatcher.InvokeAsync(SnapToNearestEdge, System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }
}
