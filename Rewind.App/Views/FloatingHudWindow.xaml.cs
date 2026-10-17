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
            Top = workingArea.Top + 140;
            DockSecretlyToEdge(animate: false);
        }

        /// <summary>
        /// Positions the mascot half-off the screen edge, secretly peeking inward
        /// </summary>
        private void DockSecretlyToEdge(bool animate = true)
        {
            _isQuickBallExpanded = false;

            QuickBallCapsule.Visibility = Visibility.Collapsed;
            PeekingMascotCapsule.Visibility = Visibility.Visible;

            UpdateLayout();

            var workingArea = SystemParameters.WorkArea;
            var w = ActualWidth > 0 ? ActualWidth : 86;
            var h = ActualHeight > 0 ? ActualHeight : 90;

            // Half-out screen peek offsets: 
            // In WPF, window Left/Top can be positioned so only half of the 54x58 mascot is on screen
            double targetLeft = Left;
            double targetTop = Top;

            // Clamping Top to avoid disappearing above or below the screen
            if (targetTop < workingArea.Top + 20) targetTop = workingArea.Top + 20;
            if (targetTop + h > workingArea.Bottom - 20) targetTop = workingArea.Bottom - h - 20;

            // Half peeking offset: Keep ~26px of the mascot on screen, rest half outside
            switch (_currentEdge)
            {
                case DockEdge.Right:
                    targetLeft = workingArea.Right - 42;
                    MascotPeekScale.ScaleX = 1.0; // Facing screen
                    break;
                case DockEdge.Left:
                    targetLeft = workingArea.Left - (w - 42);
                    MascotPeekScale.ScaleX = -1.0; // Mirrored facing inward to screen
                    break;
                case DockEdge.Top:
                    targetTop = workingArea.Top - (h - 44);
                    break;
                case DockEdge.Bottom:
                    targetTop = workingArea.Bottom - 44;
                    break;
            }

            if (animate)
            {
                var sb = new Storyboard();
                var animX = new DoubleAnimation(Left, targetLeft, new Duration(TimeSpan.FromMilliseconds(280)))
                {
                    EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
                };
                var animY = new DoubleAnimation(Top, targetTop, new Duration(TimeSpan.FromMilliseconds(280)))
                {
                    EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(animX, this);
                Storyboard.SetTargetProperty(animX, new PropertyPath(Window.LeftProperty));
                Storyboard.SetTarget(animY, this);
                Storyboard.SetTargetProperty(animY, new PropertyPath(Window.TopProperty));

                sb.Children.Add(animX);
                sb.Children.Add(animY);
                sb.Begin();
            }
            else
            {
                Left = targetLeft;
                Top = targetTop;
            }
        }

        /// <summary>
        /// Emerges from the edge into a floating Quick Ball action bar
        /// </summary>
        private void ExpandQuickBall()
        {
            if (_isQuickBallExpanded) return;
            _isQuickBallExpanded = true;

            PeekingMascotCapsule.Visibility = Visibility.Collapsed;
            QuickBallCapsule.Visibility = Visibility.Visible;

            UpdateLayout();

            var workingArea = SystemParameters.WorkArea;
            var w = ActualWidth > 0 ? ActualWidth : 340;
            var h = ActualHeight > 0 ? ActualHeight : 65;

            // Ensure Quick Ball is comfortably floating on screen away from edge
            double targetLeft = Left;
            double targetTop = Top;

            if (_currentEdge == DockEdge.Right)
            {
                targetLeft = workingArea.Right - w - 24;
            }
            else if (_currentEdge == DockEdge.Left)
            {
                targetLeft = workingArea.Left + 24;
            }

            if (targetTop < workingArea.Top + 24) targetTop = workingArea.Top + 24;
            if (targetTop + h > workingArea.Bottom - 24) targetTop = workingArea.Bottom - h - 24;

            // Smooth expansion animation
            Left = targetLeft;
            Top = targetTop;

            QuickBallScale.ScaleX = 0.6;
            QuickBallScale.ScaleY = 0.6;

            var scaleAnimX = new DoubleAnimation(0.6, 1.0, new Duration(TimeSpan.FromMilliseconds(260)))
            {
                EasingFunction = new BackEase { Amplitude = 0.35, EasingMode = EasingMode.EaseOut }
            };
            var scaleAnimY = new DoubleAnimation(0.6, 1.0, new Duration(TimeSpan.FromMilliseconds(260)))
            {
                EasingFunction = new BackEase { Amplitude = 0.35, EasingMode = EasingMode.EaseOut }
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
                    // Clicked: emerge smoothly from the screen edge into Quick Ball
                    ExpandQuickBall();
                }

                _isDragging = false;
                e.Handled = true;
            }
        }

        private void DetermineNearestEdgeAndDock()
        {
            var workingArea = SystemParameters.WorkArea;
            var w = ActualWidth > 0 ? ActualWidth : 86;
            var h = ActualHeight > 0 ? ActualHeight : 90;

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
                        // Update nearest edge after user drags the quick ball
                        var workingArea = SystemParameters.WorkArea;
                        var w = ActualWidth > 0 ? ActualWidth : 340;
                        var distLeft = Math.Abs(Left - workingArea.Left);
                        var distRight = Math.Abs((workingArea.Right - w) - Left);
                        _currentEdge = (distLeft < distRight) ? DockEdge.Left : DockEdge.Right;
                    }
                    catch { }
                }
            }
        }

        private void QuickBallMascot_Click(object sender, RoutedEventArgs e)
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
