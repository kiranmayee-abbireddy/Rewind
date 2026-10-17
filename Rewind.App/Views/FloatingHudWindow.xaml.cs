using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Rewind.App.Views
{
    public partial class FloatingHudWindow : Window
    {
        public event Action? OnPreserveRequested;
        public event Action? OnOpenMainRequested;
        public event Action? OnPauseResumeRequested;
        public event Action<bool>? OnVisibilityChanged;

        private enum DockEdge { Left, Right, Top, Bottom }
        private DockEdge _currentEdge = DockEdge.Right;
        private bool _isQuickBallExpanded = false;

        private Point _mouseDownScreenPos;
        private Point _windowStartPos;
        private bool _isDragging = false;
        private bool _isPaused = false;

        public FloatingHudWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => InitializePosition();
            IsVisibleChanged += (s, e) => OnVisibilityChanged?.Invoke(IsVisible);
        }

        public void ShowAndExpand()
        {
            Show();
            Activate();
            ExpandQuickBall();
        }

        public void UpdatePauseState(bool isPaused)
        {
            _isPaused = isPaused;
            Dispatcher.Invoke(() =>
            {
                if (PauseResumeIcon != null)
                {
                    PauseResumeIcon.Data = (Geometry)FindResource(_isPaused ? "PlayGeometry" : "PauseGeometry");
                }
            });
        }

        private void InitializePosition()
        {
            var workingArea = SystemParameters.WorkArea;
            _currentEdge = DockEdge.Right;
            Top = workingArea.Top + 160;
            DockSecretlyToEdge(animate: false);
        }

        /// <summary>
        /// Positions the mascot inclined (tilted) half-off the screen edge, secretly peeking inward
        /// </summary>
        private void DockSecretlyToEdge(bool animate = true)
        {
            _isQuickBallExpanded = false;

            QuickBallCapsule.Visibility = Visibility.Collapsed;
            PeekingMascotCapsule.Visibility = Visibility.Visible;

            UpdateLayout();

            // Clear any previous animations holding Left/Top
            BeginAnimation(Window.LeftProperty, null);
            BeginAnimation(Window.TopProperty, null);

            var workingArea = SystemParameters.WorkArea;
            var w = ActualWidth > 0 ? ActualWidth : (54 + 48);
            var h = ActualHeight > 0 ? ActualHeight : (60 + 48);

            double targetLeft = Left;
            double targetTop = Top;

            // Clamping Top to avoid disappearing above or below the screen
            if (targetTop < workingArea.Top + 20) targetTop = workingArea.Top + 20;
            if (targetTop + h > workingArea.Bottom - 20) targetTop = workingArea.Bottom - h - 20;

            // Inclined angle: when on Right edge, tilt inward (-28°). When on Left edge, mirror and tilt (+28°).
            switch (_currentEdge)
            {
                case DockEdge.Right:
                    targetLeft = workingArea.Right - 55;
                    MascotPeekScale.ScaleX = 1.0;
                    MascotPeekRotate.Angle = -28;
                    break;
                case DockEdge.Left:
                    targetLeft = workingArea.Left - (w - 55);
                    MascotPeekScale.ScaleX = -1.0;
                    MascotPeekRotate.Angle = 28;
                    break;
                case DockEdge.Top:
                    targetTop = workingArea.Top - (h - 55);
                    MascotPeekRotate.Angle = 0;
                    break;
                case DockEdge.Bottom:
                    targetTop = workingArea.Bottom - 55;
                    MascotPeekRotate.Angle = 0;
                    break;
            }

            if (animate)
            {
                var animX = new DoubleAnimation(Left, targetLeft, new Duration(TimeSpan.FromMilliseconds(260)))
                {
                    EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
                };
                var animY = new DoubleAnimation(Top, targetTop, new Duration(TimeSpan.FromMilliseconds(260)))
                {
                    EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
                };

                animX.Completed += (s, e) =>
                {
                    BeginAnimation(Window.LeftProperty, null);
                    Left = targetLeft;
                };
                animY.Completed += (s, e) =>
                {
                    BeginAnimation(Window.TopProperty, null);
                    Top = targetTop;
                };

                BeginAnimation(Window.LeftProperty, animX);
                BeginAnimation(Window.TopProperty, animY);
            }
            else
            {
                Left = targetLeft;
                Top = targetTop;
            }
        }

        /// <summary>
        /// Emerges from the edge into a fully straight mascot with floating circular options around it (no background)
        /// </summary>
        private void ExpandQuickBall()
        {
            if (_isQuickBallExpanded) return;
            _isQuickBallExpanded = true;

            PeekingMascotCapsule.Visibility = Visibility.Collapsed;
            QuickBallCapsule.Visibility = Visibility.Visible;

            UpdateLayout();

            // Clear any active edge docking animations
            BeginAnimation(Window.LeftProperty, null);
            BeginAnimation(Window.TopProperty, null);

            var workingArea = SystemParameters.WorkArea;
            // Full quick ball diameter is 240x220 + 48 padding
            var w = ActualWidth > 0 ? ActualWidth : (240 + 48);
            var h = ActualHeight > 0 ? ActualHeight : (220 + 48);

            // Shift position cleanly on-screen so the floating orbit is fully visible
            double startLeft = Left;
            double startTop = Top;
            double targetLeft = startLeft;
            double targetTop = startTop;

            if (_currentEdge == DockEdge.Right)
            {
                targetLeft = workingArea.Right - w - 24;
            }
            else if (_currentEdge == DockEdge.Left)
            {
                targetLeft = workingArea.Left + 24;
            }

            if (targetTop < workingArea.Top + 20) targetTop = workingArea.Top + 20;
            if (targetTop + h > workingArea.Bottom - 20) targetTop = workingArea.Bottom - h - 20;

            // Animate window position smoothly onto screen
            var animX = new DoubleAnimation(startLeft, targetLeft, new Duration(TimeSpan.FromMilliseconds(260)))
            {
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };
            var animY = new DoubleAnimation(startTop, targetTop, new Duration(TimeSpan.FromMilliseconds(260)))
            {
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            animX.Completed += (s, e) =>
            {
                BeginAnimation(Window.LeftProperty, null);
                Left = targetLeft;
            };
            animY.Completed += (s, e) =>
            {
                BeginAnimation(Window.TopProperty, null);
                Top = targetTop;
            };

            BeginAnimation(Window.LeftProperty, animX);
            BeginAnimation(Window.TopProperty, animY);

            // Spring pop-in animation
            QuickBallScale.ScaleX = 0.5;
            QuickBallScale.ScaleY = 0.5;

            var scaleAnimX = new DoubleAnimation(0.5, 1.0, new Duration(TimeSpan.FromMilliseconds(280)))
            {
                EasingFunction = new BackEase { Amplitude = 0.45, EasingMode = EasingMode.EaseOut }
            };
            var scaleAnimY = new DoubleAnimation(0.5, 1.0, new Duration(TimeSpan.FromMilliseconds(280)))
            {
                EasingFunction = new BackEase { Amplitude = 0.45, EasingMode = EasingMode.EaseOut }
            };

            QuickBallScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimX);
            QuickBallScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimY);
        }

        private void PeekingMascot_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                OnOpenMainRequested?.Invoke();
                e.Handled = true;
                return;
            }

            _mouseDownScreenPos = PointToScreen(e.GetPosition(this));
            _windowStartPos = new Point(Left, Top);
            _isDragging = false;
            PeekingMascotCapsule.CaptureMouse();
            e.Handled = true;
        }

        private void PeekingMascot_MouseMove(object sender, MouseEventArgs e)
        {
            if (PeekingMascotCapsule.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentScreen = PointToScreen(e.GetPosition(this));
                var dx = currentScreen.X - _mouseDownScreenPos.X;
                var dy = currentScreen.Y - _mouseDownScreenPos.Y;

                if (!_isDragging && (Math.Abs(dx) > 4 || Math.Abs(dy) > 4))
                {
                    _isDragging = true;
                }

                if (_isDragging)
                {
                    Left = _windowStartPos.X + dx;
                    Top = _windowStartPos.Y + dy;
                }
            }
        }

        private void PeekingMascot_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (PeekingMascotCapsule.IsMouseCaptured)
            {
                PeekingMascotCapsule.ReleaseMouseCapture();

                if (_isDragging)
                {
                    DetermineNearestEdgeAndDock();
                }
                else
                {
                    // Clicked: emerge cleanly and show floating circular options
                    ExpandQuickBall();
                }

                _isDragging = false;
                e.Handled = true;
            }
        }

        private void DetermineNearestEdgeAndDock()
        {
            var workingArea = SystemParameters.WorkArea;
            var w = ActualWidth > 0 ? ActualWidth : 100;

            var distLeft = Math.Abs(Left - workingArea.Left);
            var distRight = Math.Abs((workingArea.Right - w) - Left);

            _currentEdge = (distLeft < distRight) ? DockEdge.Left : DockEdge.Right;
            DockSecretlyToEdge(animate: true);
        }

        private void PeekingMascot_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Hide();
            e.Handled = true;
        }

        private void QuickBall_MouseDown(object sender, MouseButtonEventArgs e)
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
                        var workingArea = SystemParameters.WorkArea;
                        var w = ActualWidth > 0 ? ActualWidth : 288;
                        var distLeft = Math.Abs(Left - workingArea.Left);
                        var distRight = Math.Abs((workingArea.Right - w) - Left);
                        _currentEdge = (distLeft < distRight) ? DockEdge.Left : DockEdge.Right;
                    }
                    catch { }
                }
            }
        }

        private void CenterMascot_Click(object sender, RoutedEventArgs e)
        {
            DockSecretlyToEdge(animate: true);
        }

        private void DockToEdge_Click(object sender, RoutedEventArgs e)
        {
            DockSecretlyToEdge(animate: true);
        }

        public void UpdateStatus(string state, double elapsedSeconds)
        {
            Dispatcher.Invoke(() =>
            {
                var span = TimeSpan.FromSeconds(elapsedSeconds);
                BufferTimeText.Text = $"{span.Minutes:00}:{span.Seconds:00}";

                Brush statusBrush;
                string statusLabel;

                if (state == "Recording")
                {
                    statusBrush = (Brush)FindResource("DangerBrush");
                    statusLabel = "REC";
                    UpdatePauseState(false);
                }
                else if (state == "Paused")
                {
                    statusBrush = (Brush)FindResource("WarningBrush");
                    statusLabel = "PAUSE";
                    UpdatePauseState(true);
                }
                else
                {
                    statusBrush = (Brush)FindResource("TextSecondaryBrush");
                    statusLabel = "IDLE";
                    UpdatePauseState(false);
                }

                StatusText.Text = statusLabel;
                StatusText.Foreground = statusBrush;
            });
        }

        private void PreserveButton_Click(object sender, RoutedEventArgs e)
        {
            OnPreserveRequested?.Invoke();
        }

        private void PauseResumeButton_Click(object sender, RoutedEventArgs e)
        {
            OnPauseResumeRequested?.Invoke();
        }

        private void OpenMain_Click(object sender, RoutedEventArgs e)
        {
            OnOpenMainRequested?.Invoke();
        }

        private void HideHud_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
