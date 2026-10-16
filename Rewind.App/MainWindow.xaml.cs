using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Rewind.App.Models;
using Rewind.App.Services;
using Rewind.App.Views;

namespace Rewind.App
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _dbService;
        private readonly CaptureEngine _captureEngine;
        private readonly RuleBasedLocalAiAssistant _aiAssistant;
        private readonly FloatingHudWindow _hudWindow;
        private readonly DispatcherTimer _statusTimer;

        private Session? _activeSession;
        public ObservableCollection<Session> Sessions { get; } = new();
        public ObservableCollection<Bug> Bugs { get; } = new();

        public MainWindow()
        {
            InitializeComponent();

            _dbService = new DatabaseService();
            _aiAssistant = new RuleBasedLocalAiAssistant();

            var bufferDir = Path.Combine(_dbService.BaseDirectory, "buffer");
            _captureEngine = new CaptureEngine(bufferDir, bufferSeconds: 60, fps: 15);
            _captureEngine.StateChanged += OnCaptureStateChanged;

            _hudWindow = new FloatingHudWindow();
            _hudWindow.OnPreserveRequested += OnHudPreserveRequested;
            _hudWindow.OnOpenMainRequested += () =>
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            };

            // Setup timer to update status UI
            _statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _statusTimer.Tick += StatusTimer_Tick;
            _statusTimer.Start();

            // Load sessions or create initial
            LoadSessions();

            Closed += (s, e) =>
            {
                _captureEngine.Dispose();
                _hudWindow.Close();
            };
        }

        private void LoadSessions()
        {
            Sessions.Clear();
            var existing = _dbService.GetSessions();
            foreach (var s in existing)
            {
                Sessions.Add(s);
            }
            SessionsList.ItemsSource = Sessions;

            if (Sessions.Count > 0)
            {
                SelectSession(Sessions[0]);
            }
            else
            {
                CreateNewSession();
            }
        }

        private void CreateNewSession()
        {
            var session = new Session
            {
                Title = $"Session {DateTime.Now:MM/dd HH:mm}"
            };
            _dbService.SaveSession(session);
            Sessions.Insert(0, session);
            SelectSession(session);
        }

        private void SelectSession(Session session)
        {
            _activeSession = session;
            ActiveSessionTitleBox.Text = session.Title;
            LoadBugsForActiveSession();
        }

        private void LoadBugsForActiveSession()
        {
            Bugs.Clear();
            if (_activeSession != null)
            {
                var bugs = _dbService.GetBugsForSession(_activeSession.Id);
                foreach (var b in bugs)
                {
                    Bugs.Add(b);
                }
            }
            BugsList.ItemsSource = Bugs;
            BugCountLabel.Text = $"{Bugs.Count} bugs saved";
        }

        private void OnCaptureStateChanged(string state)
        {
            Dispatcher.Invoke(() =>
            {
                if (state == "Recording")
                {
                    MainStatusDot.Fill = (System.Windows.Media.Brush)FindResource("DangerBrush");
                    MainStatusLabel.Text = "RECORDING";
                    MainStatusLabel.Foreground = (System.Windows.Media.Brush)FindResource("DangerBrush");
                    StartStopCaptureBtn.Content = "⏹ Stop Capture";
                    PauseResumeBtn.IsEnabled = true;
                    PauseResumeBtn.Content = "⏸ Pause";
                    PreserveBugBtn.IsEnabled = true;
                }
                else if (state == "Paused")
                {
                    MainStatusDot.Fill = (System.Windows.Media.Brush)FindResource("WarningBrush");
                    MainStatusLabel.Text = "PAUSED";
                    MainStatusLabel.Foreground = (System.Windows.Media.Brush)FindResource("WarningBrush");
                    PauseResumeBtn.Content = "▶ Resume";
                }
                else
                {
                    MainStatusDot.Fill = (System.Windows.Media.Brush)FindResource("TextSecondaryBrush");
                    MainStatusLabel.Text = "IDLE";
                    MainStatusLabel.Foreground = (System.Windows.Media.Brush)FindResource("TextSecondaryBrush");
                    StartStopCaptureBtn.Content = "▶ Start Capture";
                    PauseResumeBtn.IsEnabled = false;
                    PreserveBugBtn.IsEnabled = false;
                }
            });
        }

        private void StatusTimer_Tick(object? sender, EventArgs e)
        {
            var elapsed = _captureEngine.CurrentElapsedSeconds;
            var state = _captureEngine.IsRunning ? (_captureEngine.IsPaused ? "Paused" : "Recording") : "Idle";

            _hudWindow.UpdateStatus(state, elapsed);

            if (_captureEngine.IsRunning)
            {
                var timeSpan = TimeSpan.FromSeconds(elapsed);
                var bufferRetained = Math.Min(60, elapsed);
                ActiveSessionSubtext.Text = $"Recording active: {timeSpan:mm\\:ss} • Retaining past {bufferRetained:0.0}s";
                BufferStatsText.Text = $"Rolling buffer active: {bufferRetained:0.0}s cached in memory & temp storage.";
            }
            else
            {
                ActiveSessionSubtext.Text = "Capture: Idle • No active recording";
                BufferStatsText.Text = "Continuous rolling buffer is stopped. Click 'Start Capture' to begin.";
            }
        }

        private void StartStopCapture_Click(object sender, RoutedEventArgs e)
        {
            if (_activeSession == null) CreateNewSession();

            if (_captureEngine.IsRunning)
            {
                _captureEngine.Stop();
                _activeSession!.Status = "Completed";
                _activeSession.EndedAt = DateTime.UtcNow;
                _dbService.SaveSession(_activeSession);
            }
            else
            {
                _captureEngine.Start();
                _activeSession!.Status = "Recording";
                _activeSession.StartedAt = DateTime.UtcNow;
                _dbService.SaveSession(_activeSession);
            }
        }

        private void PauseResume_Click(object sender, RoutedEventArgs e)
        {
            if (_captureEngine.IsPaused)
            {
                _captureEngine.Resume();
            }
            else
            {
                _captureEngine.Pause();
            }
        }

        private void PreserveBug_Click(object sender, RoutedEventArgs e)
        {
            OpenPreserveBugFlow();
        }

        private void OnHudPreserveRequested()
        {
            Dispatcher.Invoke(OpenPreserveBugFlow);
        }

        private async void OpenPreserveBugFlow()
        {
            var elapsed = _captureEngine.CurrentElapsedSeconds;
            var thumb = _captureEngine.GetLatestThumbnail();

            var dialog = new TimelineEditorDialog(elapsed, thumb);
            if (dialog.ShowDialog() == true)
            {
                if (_activeSession == null) CreateNewSession();

                var bug = new Bug
                {
                    SessionId = _activeSession!.Id,
                    Title = dialog.BugTitle,
                    ClipStartSeconds = dialog.ClipStartSeconds,
                    ClipEndSeconds = dialog.ClipEndSeconds,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Open"
                };

                if (_activeSession == null) CreateNewSession();

                // Preserving clip
                var clipsDir = Path.Combine(_dbService.BaseDirectory, "sessions", _activeSession!.Id, "clips");
                var clipFile = Path.Combine(clipsDir, $"bug_{bug.Id}_{DateTime.Now:HHmmss}.mp4");

                await _captureEngine.PreserveClipAsync(bug.ClipStartSeconds, bug.ClipEndSeconds, clipFile);
                bug.ClipPath = clipFile;
                bug.ThumbnailPath = Path.ChangeExtension(clipFile, ".jpg");

                if (dialog.EnableAiDraft && _aiAssistant.IsEnabled)
                {
                    var analysis = await _aiAssistant.AnalyzeClipAsync(clipFile, bug.DurationSeconds);
                    bug.AiAssisted = true;
                    bug.ObservedBehavior = analysis.ObservedBehavior;
                    bug.ReproductionSteps = analysis.ReproSteps;
                    bug.AiSuggestions = analysis.Suggestions;
                }

                _dbService.SaveBug(bug);
                Bugs.Add(bug);
                BugCountLabel.Text = $"{Bugs.Count} bugs saved";

                // Open Bug Detail Editor right after preservation
                var detail = new BugDetailWindow(bug);
                if (detail.ShowDialog() == true)
                {
                    if (detail.IsDeleted)
                    {
                        _dbService.DeleteBug(bug.Id);
                        Bugs.Remove(bug);
                    }
                    else
                    {
                        _dbService.SaveBug(bug);
                    }
                    LoadBugsForActiveSession();
                }
            }
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (_activeSession == null) return;
            var bugsList = _dbService.GetBugsForSession(_activeSession.Id);
            var md = ReportGenerator.GenerateMarkdownReport(_activeSession, bugsList);
            var reportWin = new ReportPreviewWindow(md);
            reportWin.ShowDialog();
        }

        private void ShowHud_Click(object sender, RoutedEventArgs e)
        {
            _hudWindow.Show();
            _hudWindow.Activate();
        }

        private void NewSession_Click(object sender, RoutedEventArgs e)
        {
            if (_captureEngine.IsRunning)
            {
                _captureEngine.Stop();
            }
            CreateNewSession();
        }

        private void SessionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SessionsList.SelectedItem is Session session)
            {
                if (_captureEngine.IsRunning)
                {
                    _captureEngine.Stop();
                }
                SelectSession(session);
            }
        }

        private void EditBug_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Bug bug)
            {
                OpenBugDetail(bug);
            }
        }

        private void BugsList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (BugsList.SelectedItem is Bug bug)
            {
                OpenBugDetail(bug);
            }
        }

        private void OpenBugDetail(Bug bug)
        {
            var detail = new BugDetailWindow(bug);
            if (detail.ShowDialog() == true)
            {
                if (detail.IsDeleted)
                {
                    _dbService.DeleteBug(bug.Id);
                    Bugs.Remove(bug);
                }
                else
                {
                    _dbService.SaveBug(bug);
                }
                LoadBugsForActiveSession();
            }
        }
    }
}