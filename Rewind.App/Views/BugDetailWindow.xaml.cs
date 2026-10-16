using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Rewind.App.Models;

namespace Rewind.App.Views
{
    public partial class BugDetailWindow : Window
    {
        private readonly Bug _bug;
        public bool IsDeleted { get; private set; }

        public BugDetailWindow(Bug bug)
        {
            InitializeComponent();
            _bug = bug;

            TitleBox.Text = bug.Title;
            MetaInfoText.Text = $"Captured at {bug.CreatedAt:HH:mm:ss} • {bug.DurationSeconds:0.0}s clip ({bug.ClipStartSeconds:0.0}s → {bug.ClipEndSeconds:0.0}s)";
            ClipPathLabel.Text = $"File: {bug.ClipPath}";
            ObservedBox.Text = bug.ObservedBehavior;
            ExpectedBox.Text = bug.ExpectedBehavior;
            ReproBox.Text = bug.ReproductionSteps;
            TagsBox.Text = bug.Tags;

            foreach (ComboBoxItem item in StatusCombo.Items)
            {
                if (item.Content.ToString() == bug.Status)
                {
                    item.IsSelected = true;
                    break;
                }
            }

            if (bug.AiAssisted && !string.IsNullOrWhiteSpace(bug.AiSuggestions))
            {
                AiContainer.Visibility = Visibility.Visible;
                AiNotesBlock.Text = bug.AiSuggestions;
            }
            else
            {
                AiContainer.Visibility = Visibility.Collapsed;
            }

            // Load thumbnail
            var thumb = !string.IsNullOrEmpty(bug.ThumbnailPath) && File.Exists(bug.ThumbnailPath)
                ? bug.ThumbnailPath
                : Path.ChangeExtension(bug.ClipPath, ".jpg");

            if (File.Exists(thumb))
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(thumb, UriKind.Absolute);
                    bmp.EndInit();
                    ThumbnailImage.Source = bmp;
                }
                catch { }
            }
        }

        private void PlayVideo_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(_bug.ClipPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _bug.ClipPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not launch media player: {ex.Message}", "Playback Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Clip video file was not found on disk.", "File Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show($"Are you sure you want to permanently delete bug '{_bug.Title}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                IsDeleted = true;
                DialogResult = true;
                Close();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _bug.Title = string.IsNullOrWhiteSpace(TitleBox.Text) ? _bug.Title : TitleBox.Text.Trim();
            _bug.ObservedBehavior = ObservedBox.Text.Trim();
            _bug.ExpectedBehavior = ExpectedBox.Text.Trim();
            _bug.ReproductionSteps = ReproBox.Text.Trim();
            _bug.Tags = TagsBox.Text.Trim();
            _bug.Status = ((ComboBoxItem)StatusCombo.SelectedItem)?.Content?.ToString() ?? "Open";

            DialogResult = true;
            Close();
        }
    }
}
