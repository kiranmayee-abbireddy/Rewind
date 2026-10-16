using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace Rewind.App.Views
{
    public partial class ReportPreviewWindow : Window
    {
        private string _cleanListReport;
        private string _markdownReport;
        private string _jiraReport;
        private string _jsonReport;

        public ReportPreviewWindow(string cleanListContent, string? markdownContent = null, string? jiraContent = null, string? jsonContent = null)
        {
            InitializeComponent();
            _cleanListReport = cleanListContent;
            _markdownReport = markdownContent ?? cleanListContent;
            _jiraReport = jiraContent ?? cleanListContent;
            _jsonReport = jsonContent ?? "{}";

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
