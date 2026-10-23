using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using Rewind.App.Models;
using Rewind.App.Services;

namespace Rewind.App.Views
{
    public partial class ReportPreviewWindow : Window
    {
        private string _cleanListReport = "";
        private string _markdownReport = "";
        private string _jiraReport = "";
        private string _jsonReport = "";

        private readonly Session? _session;
        private readonly List<Bug>? _bugs;
        private readonly IAiAssistantService? _aiAssistant;
        private Storyboard? _loadingStoryboard;

        /// <summary>
        /// Constructor for pre-computed reports (legacy/instant fallback)
        /// </summary>
        public ReportPreviewWindow(string cleanListContent, string? markdownContent = null, string? jiraContent = null, string? jsonContent = null)
        {
            InitializeComponent();
            _cleanListReport = cleanListContent;
            _markdownReport = markdownContent ?? cleanListContent;
            _jiraReport = jiraContent ?? cleanListContent;
            _jsonReport = jsonContent ?? "{}";

            ReportBox.Text = _cleanListReport;

            // Direct display without loading overlay
            LoadingOverlay.Visibility = Visibility.Collapsed;
            ReportContentGrid.Visibility = Visibility.Visible;
            ReportContentGrid.Opacity = 1.0;
        }

        /// <summary>
        /// Constructor that opens the window immediately and generates the reports asynchronously with animations.
        /// </summary>
        public ReportPreviewWindow(Session session, List<Bug> bugs, IAiAssistantService? aiAssistant = null)
        {
            InitializeComponent();
            _session = session;
            _bugs = bugs;
            _aiAssistant = aiAssistant;

            Loaded += ReportPreviewWindow_Loaded;
        }

        private async void ReportPreviewWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StartLoadingAnimations();

            try
            {
                await LoadReportDataAsync();
            }
            catch (Exception ex)
            {
                LoadingStatusText.Text = $"Error generating report: {ex.Message}";
                return;
            }

            TransitionToContent();
        }

        private void StartLoadingAnimations()
        {
            _loadingStoryboard = new Storyboard();

            // 1. Shimmer Translate Animation (Infinite loop across bar)
            var shimmerAnim = new DoubleAnimation
            {
                From = -140,
                To = 440,
                Duration = new Duration(TimeSpan.FromSeconds(1.6)),
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(shimmerAnim, IndeterminateBar);
            Storyboard.SetTargetProperty(shimmerAnim, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            _loadingStoryboard.Children.Add(shimmerAnim);

            // 2. Avocado Mascot gentle float / bounce animation
            var bounceAnim = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            bounceAnim.KeyFrames.Add(new SplineDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            bounceAnim.KeyFrames.Add(new SplineDoubleKeyFrame(-8, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.75))));
            bounceAnim.KeyFrames.Add(new SplineDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.5))));
            Storyboard.SetTarget(bounceAnim, MascotTranslate);
            Storyboard.SetTargetProperty(bounceAnim, new PropertyPath("Y"));
            _loadingStoryboard.Children.Add(bounceAnim);

            // 3. Glow Ring Pulsing Scale & Opacity Animation
            var glowScaleXAnim = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            glowScaleXAnim.KeyFrames.Add(new SplineDoubleKeyFrame(0.9, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            glowScaleXAnim.KeyFrames.Add(new SplineDoubleKeyFrame(1.25, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.75))));
            glowScaleXAnim.KeyFrames.Add(new SplineDoubleKeyFrame(0.9, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.5))));
            Storyboard.SetTarget(glowScaleXAnim, GlowRing);
            Storyboard.SetTargetProperty(glowScaleXAnim, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            _loadingStoryboard.Children.Add(glowScaleXAnim);

            var glowScaleYAnim = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            glowScaleYAnim.KeyFrames.Add(new SplineDoubleKeyFrame(0.9, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            glowScaleYAnim.KeyFrames.Add(new SplineDoubleKeyFrame(1.25, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.75))));
            glowScaleYAnim.KeyFrames.Add(new SplineDoubleKeyFrame(0.9, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.5))));
            Storyboard.SetTarget(glowScaleYAnim, GlowRing);
            Storyboard.SetTargetProperty(glowScaleYAnim, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            _loadingStoryboard.Children.Add(glowScaleYAnim);

            _loadingStoryboard.Begin(this, isControllable: true);
        }

        private async Task LoadReportDataAsync()
        {
            if (_session == null || _bugs == null) return;

            // Step 1: Status update & slight yield so UI animation immediately renders smoothly
            LoadingStatusText.Text = "Cataloging session clips & bug metadata...";
            await Task.Delay(400);

            string? aiSummary = null;
            if (_aiAssistant != null && _aiAssistant.IsEnabled && _bugs.Count > 0)
            {
                LoadingStatusText.Text = "Synthesizing AI executive summary & insights...";
                var sbTitles = new System.Text.StringBuilder();
                for (int i = 0; i < _bugs.Count; i++)
                {
                    sbTitles.AppendLine($"• Bug #{i + 1}: {_bugs[i].Title} (Status: {_bugs[i].Status})");
                }

                try
                {
                    aiSummary = await _aiAssistant.SummarizeSessionReportAsync(_session.Title, _bugs.Count, sbTitles.ToString());
                }
                catch
                {
                    // Fail gracefully without breaking report generation
                }
            }

            LoadingStatusText.Text = "Formatting report in Clean, Markdown, Jira & JSON...";
            await Task.Delay(250);

            // Compute the formatted reports in background task
            await Task.Run(() =>
            {
                _cleanListReport = ReportGenerator.GenerateCleanListReport(_session, _bugs);
                _markdownReport = ReportGenerator.GenerateMarkdownReport(_session, _bugs, aiExecutiveSummary: aiSummary);
                _jiraReport = ReportGenerator.GenerateJiraFormat(_session, _bugs);
                _jsonReport = ReportGenerator.GenerateJsonReport(_session, _bugs, aiExecutiveSummary: aiSummary);
            });

            ReportBox.Text = _cleanListReport;
        }

        private void FormatCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ReportBox == null) return;

            ReportBox.Text = FormatCombo.SelectedIndex switch
            {
                1 => _markdownReport,
                2 => _jiraReport,
                3 => _jsonReport,
                _ => _cleanListReport
            };
            CopyStatusText.Text = "";
        }

        private void TransitionToContent()
        {
            _loadingStoryboard?.Stop(this);

            // Smooth cross-fade transition: fade out loading overlay, fade in content grid
            ReportContentGrid.Visibility = Visibility.Visible;

            var fadeOut = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(300)));
            fadeOut.Completed += (s, ev) =>
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            };
            LoadingOverlay.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            var fadeIn = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(400)))
            {
                BeginTime = TimeSpan.FromMilliseconds(100)
            };
            ReportContentGrid.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ReportBox.Text);
            CopyStatusText.Text = "✔ Copied to clipboard!";
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            var ext = FormatCombo.SelectedIndex switch
            {
                1 => "md",
                2 => "txt",
                3 => "json",
                _ => "txt"
            };

            var sfd = new SaveFileDialog
            {
                Filter = $"{ext.ToUpper()} files (*.{ext})|*.{ext}|All files (*.*)|*.*",
                FileName = $"QA_Report_{DateTime.Now:yyyyMMdd_HHmm}.{ext}"
            };

            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, ReportBox.Text);
                CopyStatusText.Text = "✔ Saved successfully!";
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
