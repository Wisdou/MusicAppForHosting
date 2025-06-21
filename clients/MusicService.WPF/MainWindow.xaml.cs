using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MusicService.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;

    public MainWindow()
    {
        InitializeComponent();
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _timer.Tick += UpdateProgress;
        MediaElement.SpeedRatio = 1.0;
        ComboBoxSpeed.SelectedValue = 1.0;
    }

    
    private void OnStopClick(object sender, RoutedEventArgs e)
    {
        MediaElement.Stop();
        MediaElement.Close();
        MediaElement.Source = null;
        
        _timer.Stop();
        ProgressSlider.Value = 0;
        CurrentTimeText.Text = "00:00";
    }

    private void OnRewindClick(object sender, RoutedEventArgs e)
    {
        MediaElement.Position -= TimeSpan.FromSeconds(10);
    }

    private void OnForwardClick(object sender, RoutedEventArgs e)
    {
        MediaElement.Position += TimeSpan.FromSeconds(10);
    }

    private void OnVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        MediaElement.Volume = VolumeSlider.Value;
    }

    private void OnProgressChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (MediaElement.NaturalDuration.HasTimeSpan)
        {
            MediaElement.Position = TimeSpan.FromSeconds(MediaElement.NaturalDuration.TimeSpan.TotalSeconds * ProgressSlider.Value / 100);
        }
    }

    private void UpdateProgress(object? sender, EventArgs e)
    {
        if (!MediaElement.NaturalDuration.HasTimeSpan)
        {
            return;
        }
        
        ProgressSlider.Value = MediaElement.Position.TotalSeconds / MediaElement.NaturalDuration.TimeSpan.TotalSeconds * 100;
        CurrentTimeText.Text = MediaElement.Position.ToString(@"mm\:ss");
        TotalTimeText.Text = MediaElement.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
    }

    private void OnPlayClicked(object sender, RoutedEventArgs e)
    {
        _timer.Start();
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox)
        {
            var selectedItem = comboBox.SelectedItem;
            MediaElement.SpeedRatio = (double)selectedItem;
        }
    }
}