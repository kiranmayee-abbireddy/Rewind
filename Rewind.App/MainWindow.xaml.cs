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
        private IAiAssistantService _aiAssistant;
        private readonly FloatingHudWindow _hudWindow;
        private readonly DispatcherTimer _statusTimer;
        private readonly GlobalHotkeyManager _hotkeyManager;
        private AppSettings _appSettings = new();

        private Session? _activeSession;
        public ObservableCollection<Session> Sessions { get; } = new();
        public ObservableCollection<Bug> Bugs { get; } = new();

        public MainWindow()
        {
            InitializeComponent();

            _dbService = new DatabaseService();
            _appSettings = _dbService.LoadAppSettings();
            _aiAssistant = new OllamaAiAssistant(_appSettings.OllamaEndpoint, _appSettings.OllamaModel)
            {
                IsEnabled = _appSettings.AiEnabled
            };
            _hotkeyManager = new GlobalHotkeyManager();
            _hotkeyManager.HotkeyPressed += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (_captureEngine?.IsRunning == true)
                    {
                        OpenPreserveBugFlow();
                    }
                });
            };

            var bufferDir = Path.Combine(_dbService.BaseDirectory, "buffer");
            _captureEngine = new CaptureEngine(bufferDir, bufferSeconds: _appSettings.BufferDurationSeconds, fps: _appSettings.FrameRate);
            _captureEngine.StateChanged += OnCaptureStateChanged;

            _hudWindow = new FloatingHudWindow();
            _hudWindow.OnPreserveRequested += OnHudPreserveRequested;
            _hudWindow.OnOpenMainRequested += () =>
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            };

            Loaded += (s, e) =>
            {
                var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                _hotkeyManager.Register(handle, System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift, System.Windows.Input.Key.R);

                // Show the Floating HUD automatically so it floats on the desktop immediately
                _hudWindow.Show();
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
                _hotkeyManager.Dispose();
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
                // Crash recovery per PRD FR-06: An interrupted session must not be left as active or completed
                if (s.Status == "Recording")
                {
                    s.Status = "Interrupted (Recovered)";
                    s.EndedAt = DateTime.UtcNow;
                    _dbService.SaveSession(s);
                }
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
            if (SessionsList.SelectedItem != session)
            {
                SessionsList.SelectedItem = session;
            }
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
                    MainStatusDot.Background = (System.Windows.Media.Brush)FindResource("DangerBrush");
                    MainStatusLabel.Text = "RECORDING";
                    MainStatusLabel.Foreground = (System.Windows.Media.Brush)FindResource("DangerBrush");
                    StartStopCaptureBtn.Content = "⏹ Stop Capture";
                    PauseResumeBtn.IsEnabled = true;
                    PauseResumeBtn.Content = "⏸ Pause";
                    PreserveBugBtn.IsEnabled = true;
                }
                else if (state == "Paused")
                {
                    MainStatusDot.Background = (System.Windows.Media.Brush)FindResource("WarningBrush");
                    MainStatusLabel.Text = "PAUSED";
                    MainStatusLabel.Foreground = (System.Windows.Media.Brush)FindResource("WarningBrush");
                    PauseResumeBtn.Content = "▶ Resume";
                }
                else
                {
                    MainStatusDot.Background = (System.Windows.Media.Brush)FindResource("TextSecondaryBrush");
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

            var dialog = new TimelineEditorDialog(elapsed, thumb, _captureEngine.GetFrameAtTime);
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

                // Update UI state to show processing and ask user to wait
                ProcessingStatusText.Text = "⏳ Encoding MP4 video clip & applying redactions... please wait.";
                PreservationProgressBar.Visibility = Visibility.Visible;
                PreservationProgressBar.IsIndeterminate = true;
                PreserveBugBtn.IsEnabled = false;

                try
                {
                    await Task.Run(async () =>
                    {
                        await _captureEngine.PreserveClipAsync(bug.ClipStartSeconds, bug.ClipEndSeconds, clipFile, dialog.NormalizedRedactions);
                    });

                    bug.ClipPath = clipFile;
                    bug.ThumbnailPath = Path.ChangeExtension(clipFile, ".jpg");

                    if (dialog.EnableAiDraft && _aiAssistant.IsEnabled)
                    {
                        ProcessingStatusText.Text = $"🤖 Querying {_aiAssistant.ProviderName} to analyze footage and rewrite title... please wait.";
                        var userContext = !string.IsNullOrWhiteSpace(dialog.TestCaseContext) ? dialog.TestCaseContext : dialog.BugTitle;
                        var analysis = await _aiAssistant.AnalyzeClipAsync(clipFile, bug.DurationSeconds, userContext);
                        bug.AiAssisted = true;

                        // AI synthesizes and rewrites the bug title into a professional defect title
                        if (!string.IsNullOrWhiteSpace(analysis.Title))
                        {
                            bug.Title = analysis.Title;
                        }

                        bug.ObservedBehavior = analysis.ObservedBehavior;
                        bug.ReproductionSteps = analysis.ReproSteps;
                        bug.AiSuggestions = analysis.Suggestions;
                    }

                    // Rename clip and thumbnail so the video file on disk matches the bug title
                    try
                    {
                        var sanitizedTitle = SanitizeFileName(bug.Title);
                        if (string.IsNullOrWhiteSpace(sanitizedTitle)) sanitizedTitle = $"bug_{bug.Id}";
                        var namedClipFile = Path.Combine(clipsDir, $"{sanitizedTitle}_{bug.Id.Substring(0, 8)}.mp4");
                        var namedThumbFile = Path.ChangeExtension(namedClipFile, ".jpg");

                        if (!clipFile.Equals(namedClipFile, StringComparison.OrdinalIgnoreCase) && File.Exists(clipFile))
                        {
                            if (File.Exists(namedClipFile)) File.Delete(namedClipFile);
                            File.Move(clipFile, namedClipFile);
                            bug.ClipPath = namedClipFile;

                            var oldThumb = Path.ChangeExtension(clipFile, ".jpg");
                            if (File.Exists(oldThumb))
                            {
                                if (File.Exists(namedThumbFile)) File.Delete(namedThumbFile);
                                File.Move(oldThumb, namedThumbFile);
                                bug.ThumbnailPath = namedThumbFile;
                            }
                        }
                    }
                    catch { }

                    _dbService.SaveBug(bug);
                    Bugs.Add(bug);
                    BugCountLabel.Text = $"{Bugs.Count} bugs saved";
                    ProcessingStatusText.Text = $"✔ Bug '{bug.Title}' successfully preserved with immutable video evidence!";

                    // Ensure SessionsList selection is visually synchronized with the active session
                    SessionsList.SelectedItem = _activeSession;

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
                            // If title was updated in detail window, keep video file name synchronized
                            try
                            {
                                var updatedSanitized = SanitizeFileName(bug.Title);
                                if (!string.IsNullOrWhiteSpace(updatedSanitized))
                                {
                                    var updatedClip = Path.Combine(clipsDir, $"{updatedSanitized}_{bug.Id.Substring(0, 8)}.mp4");
                                    if (!bug.ClipPath.Equals(updatedClip, StringComparison.OrdinalIgnoreCase) && File.Exists(bug.ClipPath))
                                    {
                                        if (File.Exists(updatedClip)) File.Delete(updatedClip);
                                        File.Move(bug.ClipPath, updatedClip);
                                        bug.ClipPath = updatedClip;

                                        var oldThumb = Path.ChangeExtension(bug.ClipPath, ".jpg");
                                        var newThumb = Path.ChangeExtension(updatedClip, ".jpg");
                                        if (File.Exists(oldThumb))
                                        {
                                            if (File.Exists(newThumb)) File.Delete(newThumb);
                                            File.Move(oldThumb, newThumb);
                                            bug.ThumbnailPath = newThumb;
                                        }
                                    }
                                }
                            }
                            catch { }

                            _dbService.SaveBug(bug);
                        }
                        LoadBugsForActiveSession();
                    }
                }
                catch (Exception ex)
                {
                    ProcessingStatusText.Text = $"❌ Preservation failed: {ex.Message}";
                    MessageBox.Show($"Could not preserve bug footage: {ex.Message}", "Clip Preservation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                finally
                {
                    PreservationProgressBar.Visibility = Visibility.Collapsed;
                    PreservationProgressBar.IsIndeterminate = false;
                    PreserveBugBtn.IsEnabled = _captureEngine.IsRunning;
                }
            }
        }

        private async void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (_activeSession == null) return;
            var bugsList = _dbService.GetBugsForSession(_activeSession.Id);

            // If active session has 0 bugs, check if other sessions have preserved bugs
            if (bugsList.Count == 0)
            {
                var sessionsWithBugs = Sessions.Where(s => _dbService.GetBugsForSession(s.Id).Count > 0).ToList();
                if (sessionsWithBugs.Count > 0)
                {
                    var targetSession = sessionsWithBugs[0];
                    var result = MessageBox.Show(
                        $"The currently selected session '{_activeSession.Title}' has 0 bugs saved.\n\nWould you like to generate the report for '{targetSession.Title}', which contains {_dbService.GetBugsForSession(targetSession.Id).Count} preserved bug(s)?",
                        "Generate Report — Select Session",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        SelectSession(targetSession);
                        bugsList = _dbService.GetBugsForSession(targetSession.Id);
                    }
                    else
                    {
                        // Proceed with empty report or return
                    }
                }
            }

            string? aiSummary = null;
            if (_aiAssistant.IsEnabled && bugsList.Count > 0)
            {
                var sbTitles = new System.Text.StringBuilder();
                for (int i = 0; i < bugsList.Count; i++)
                {
                    sbTitles.AppendLine($"• Bug #{i + 1}: {bugsList[i].Title} (Status: {bugsList[i].Status})");
                }
                aiSummary = await _aiAssistant.SummarizeSessionReportAsync(_activeSession.Title, bugsList.Count, sbTitles.ToString());
            }

            var cleanList = ReportGenerator.GenerateCleanListReport(_activeSession, bugsList);
            var md = ReportGenerator.GenerateMarkdownReport(_activeSession, bugsList, aiExecutiveSummary: aiSummary);
            var jira = ReportGenerator.GenerateJiraFormat(_activeSession, bugsList);
            var json = ReportGenerator.GenerateJsonReport(_activeSession, bugsList, aiExecutiveSummary: aiSummary);

            var reportWin = new ReportPreviewWindow(cleanList, md, jira, json);
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

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SettingsDialog(_appSettings);
            if (dlg.ShowDialog() == true)
            {
                _appSettings = dlg.Settings;
                _dbService.SaveAppSettings(_appSettings);
                _aiAssistant = new OllamaAiAssistant(_appSettings.OllamaEndpoint, _appSettings.OllamaModel)
                {
                    IsEnabled = _appSettings.AiEnabled
                };
                _captureEngine.CaptureSourceMode = _appSettings.CaptureSource;
                SettingsSummaryText.Text = $"Buffer: {_appSettings.BufferDurationSeconds}s @ {_appSettings.FrameRate}fps • {_appSettings.OllamaModel}";
            }
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "bug";
            var invalidChars = Path.GetInvalidFileNameChars();
            var clean = new string(name.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
            clean = clean.Replace(' ', '_').Replace('\t', '_');
            while (clean.Contains("__")) clean = clean.Replace("__", "_");
            clean = clean.Trim('_');
            if (clean.Length > 50) clean = clean.Substring(0, 50).TrimEnd('_');
            return clean.Length > 0 ? clean : "bug";
        }
    }
}