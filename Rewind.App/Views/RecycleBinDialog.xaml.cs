using System;
using System.Collections.ObjectModel;
using System.Windows;
using Rewind.App.Models;
using Rewind.App.Services;

namespace Rewind.App.Views
{
    public partial class RecycleBinDialog : Window
    {
        private readonly DatabaseService _dbService;
        public ObservableCollection<Session> DeletedSessions { get; } = new();
        public ObservableCollection<Bug> DeletedBugs { get; } = new();
        private bool _isSessionsTab = true;

        public bool ItemsChanged { get; private set; }

        public RecycleBinDialog(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            LoadBinData();
        }

        private void LoadBinData()
        {
            DeletedSessions.Clear();
            var sessions = _dbService.GetSessions(includeDeleted: true);
            foreach (var s in sessions)
            {
                DeletedSessions.Add(s);
            }
            DeletedSessionsList.ItemsSource = DeletedSessions;

            DeletedBugs.Clear();
            var bugs = _dbService.GetDeletedBugs();
            foreach (var b in bugs)
            {
                DeletedBugs.Add(b);
            }
            DeletedBugsList.ItemsSource = DeletedBugs;

            UpdateView();
        }

        private void UpdateView()
        {
            var totalCount = DeletedSessions.Count + DeletedBugs.Count;
            StatusSummaryText.Text = $"{DeletedSessions.Count} session(s), {DeletedBugs.Count} bug(s) in Recycle Bin";

            if (_isSessionsTab)
            {
                DeletedSessionsList.Visibility = DeletedSessions.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                DeletedBugsList.Visibility = Visibility.Collapsed;
                BinEmptyState.Visibility = DeletedSessions.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                DeletedSessionsList.Visibility = Visibility.Collapsed;
                DeletedBugsList.Visibility = DeletedBugs.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                BinEmptyState.Visibility = DeletedBugs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void SessionsTab_Click(object sender, RoutedEventArgs e)
        {
            _isSessionsTab = true;
            SessionsTabBtn.Opacity = 1.0;
            BugsTabBtn.Opacity = 0.6;
            UpdateView();
        }

        private void BugsTab_Click(object sender, RoutedEventArgs e)
        {
            _isSessionsTab = false;
            SessionsTabBtn.Opacity = 0.6;
            BugsTabBtn.Opacity = 1.0;
            UpdateView();
        }

        private void RestoreSession_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.Tag is Session session)
            {
                _dbService.RestoreSession(session.Id);
                ItemsChanged = true;
                LoadBinData();
            }
        }

        private void PermanentlyDeleteSession_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.Tag is Session session)
            {
                var confirm = MessageBox.Show($"Permanently delete session '{session.Title}' and all its clips? This cannot be undone.", "Permanent Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm == MessageBoxResult.Yes)
                {
                    _dbService.PermanentlyDeleteSession(session.Id);
                    ItemsChanged = true;
                    LoadBinData();
                }
            }
        }

        private void RestoreBug_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.Tag is Bug bug)
            {
                _dbService.RestoreBug(bug.Id);
                ItemsChanged = true;
                LoadBinData();
            }
        }

        private void PermanentlyDeleteBug_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.Tag is Bug bug)
            {
                var confirm = MessageBox.Show($"Permanently delete bug evidence '{bug.Title}'? This cannot be undone.", "Permanent Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm == MessageBoxResult.Yes)
                {
                    _dbService.PermanentlyDeleteBug(bug.Id);
                    ItemsChanged = true;
                    LoadBinData();
                }
            }
        }

        private void EmptyBin_Click(object sender, RoutedEventArgs e)
        {
            var totalCount = DeletedSessions.Count + DeletedBugs.Count;
            if (totalCount == 0) return;

            var confirm = MessageBox.Show($"Are you sure you want to permanently delete all {totalCount} items in the Recycle Bin? All footage and files will be permanently erased.", "Empty Recycle Bin", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm == MessageBoxResult.Yes)
            {
                _dbService.EmptyRecycleBin();
                ItemsChanged = true;
                LoadBinData();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
