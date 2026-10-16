using System.Windows;
using Rewind.App.Models;

namespace Rewind.App.Views
{
    public partial class SettingsDialog : Window
    {
        public AppSettings Settings { get; private set; }

        public SettingsDialog(AppSettings current)
        {
            InitializeComponent();
            Settings = current;

            BufferDurationSlider.Value = current.BufferDurationSeconds;
            BufferDurationValueLabel.Text = $"{current.BufferDurationSeconds}s";

            if (current.FrameRate == 30) FpsCombo.SelectedIndex = 1;
            else if (current.FrameRate == 60) FpsCombo.SelectedIndex = 2;
            else FpsCombo.SelectedIndex = 0;

            AiEnabledCheck.IsChecked = current.AiEnabled;
        }

        private void BufferDurationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (BufferDurationValueLabel != null)
            {
                BufferDurationValueLabel.Text = $"{e.NewValue:0}s";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Settings.BufferDurationSeconds = (int)BufferDurationSlider.Value;
            Settings.FrameRate = FpsCombo.SelectedIndex switch
            {
                1 => 30,
                2 => 60,
                _ => 15
            };
            Settings.AiEnabled = AiEnabledCheck.IsChecked ?? true;

            DialogResult = true;
            Close();
        }
    }
}
