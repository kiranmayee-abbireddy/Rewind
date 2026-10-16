using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace Rewind.App.Views
{
    public partial class ReportPreviewWindow : Window
    {
        private string _markdownReport;
        private string _jiraReport;
        private string _jsonReport;

        public ReportPreviewWindow(string markdownContent, string? jiraContent = null, string? jsonContent = null)
        {
            InitializeComponent();
            _markdownReport = markdownContent;
            _jiraReport = jiraContent ?? markdownContent;
            _jsonReport = jsonContent ?? "{}";

            ReportBox.Text = _markdownReport;
        }

        private void FormatCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ReportBox == null) return;

            ReportBox.Text = FormatCombo.SelectedIndex switch
            {
                1 => _jiraReport,
                2 => _jsonReport,
                _ => _markdownReport
            };
            CopyStatusText.Text = "";
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
                2 => "json",
                _ => "md"
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
    }
}
