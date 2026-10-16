using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace Rewind.App.Views
{
    public partial class ReportPreviewWindow : Window
    {
        public ReportPreviewWindow(string markdownContent)
        {
            InitializeComponent();
            ReportBox.Text = markdownContent;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ReportBox.Text);
            CopyStatusText.Text = "✔ Copied to clipboard!";
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Markdown files (*.md)|*.md|Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = $"QA_Report_{DateTime.Now:yyyyMMdd_HHmm}.md"
            };

            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, ReportBox.Text);
                CopyStatusText.Text = "✔ Saved successfully!";
            }
        }
    }
}
