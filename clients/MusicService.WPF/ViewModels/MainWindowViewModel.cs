using System.IO;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using MusicService.WPF.Services;

namespace MusicService.WPF.ViewModels;

public partial class MainWindowViewModel : ObservableRecipient, IRecipient<CloseDialogRequest>
{
    public CurrentUser? CurrentUser => App.CurrentUser;

    [ObservableProperty]
    private bool _isAddingRunning;
    
    [ObservableProperty]
    private TrackAddViewModel? _trackAddViewModel;

    [ObservableProperty] 
    private ObservableObject? _selectedPage;
    public TracksPanelViewModel TracksPanelViewModel { get; }
    
    public FavoritesPanelViewModel FavoritesPanelViewModel { get; }
    
    [ObservableProperty]
    private TrackViewModel? _selectedTrack;
    
    private bool _isRunning;
    
    [ObservableProperty]
    private TrackViewModel? _runningTrack;

    private readonly List<Type> _initialized = new();

    public MainWindowViewModel(TracksPanelViewModel tracksPanelViewModel, FavoritesPanelViewModel favoritesPanelViewModel)
    {
        TracksPanelViewModel = tracksPanelViewModel;
        FavoritesPanelViewModel = favoritesPanelViewModel;
        TracksPanelViewModel.IsActive = true;
        FavoritesPanelViewModel.IsActive = true;
        IsActive = true;
    }

    [RelayCommand]
    private static void SignOut(Window window)
    {
        if (File.Exists(App.AccessTokenFilePath))
        {
            File.Delete(App.AccessTokenFilePath);
        }
        
        App.CurrentUser = null;
        App.AccessToken = string.Empty;
        
        window.Close();
    }

    [RelayCommand]
    private void Stop()
    {
        _isRunning = false;
        RunningTrack = null;
    }

    [RelayCommand]
    private async Task DeleteTrack(TrackViewModel? track)
    {
        if (track is null)
        {
            return;
        }
        
        var dialogResult = MessageBox.Show("Вы уверены, что желаете удалить данный трек?", "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        if (dialogResult == MessageBoxResult.Yes)
        {
            using var scope = App.Services.CreateScope();
            var endpoints = scope.ServiceProvider.GetRequiredService<ITracksEndpoints>();

            using var response = await endpoints.DeleteTrack(App.AccessToken, track.Track.Id);
            if (response.IsSuccessStatusCode)
            {
                await TracksPanelViewModel.RefreshAsync();
                await FavoritesPanelViewModel.RefreshAsync();
            }
            else
            {
                MessageBox.Show(response.ReasonPhrase, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task RemoveFromFavorites(TrackViewModel? track)
    {
        if (track is null)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var endpoints = scope.ServiceProvider.GetRequiredService<ITracksEndpoints>();

        using var response = await endpoints.RemoveFromFavorites(App.AccessToken, track.Track.Id);
        if (response.IsSuccessStatusCode)
        {
            await TracksPanelViewModel.RefreshAsync();
            await FavoritesPanelViewModel.RefreshAsync();
        }
        else
        {
            MessageBox.Show(response.ReasonPhrase, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task AddToFavorites(TrackViewModel? track)
    {
        if (track is null)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var endpoints = scope.ServiceProvider.GetRequiredService<ITracksEndpoints>();

        using var response = await endpoints.AddToFavorites(App.AccessToken, track.Track.Id);
        if (response.IsSuccessStatusCode)
        {
            await TracksPanelViewModel.RefreshAsync();
            await FavoritesPanelViewModel.RefreshAsync();
        }
        else
        {
            MessageBox.Show(response.ReasonPhrase, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private async Task Play(MediaElement mediaElement)
    {
        if (SelectedTrack is null || PlayCommand.IsRunning)
        {
            return;
        }

        if (SelectedTrack.Track.Id == RunningTrack?.Track.Id)
        {
            if (_isRunning)
            {
                return;
            }
            
            mediaElement.Play();
            _isRunning = true;

            return;
        }
        
        mediaElement.Stop();
        mediaElement.Close();
        mediaElement.Source = null;

        var filePath = Path.Combine(Path.GetTempPath(), $"{SelectedTrack.Track.Id}.mp3");
        if (File.Exists(filePath))
        {
            mediaElement.Source = new Uri(filePath);
            mediaElement.Play();
            RunningTrack = SelectedTrack;
            return;
        }

        using var scope = App.Services.CreateScope();
        var endpoints = scope.ServiceProvider.GetRequiredService<ITracksEndpoints>();

        using var audiResp = await endpoints.LoadAudio(App.AccessToken, SelectedTrack.Track.Id);

        if (audiResp.IsSuccessStatusCode)
        {
            var fileStream = new FileStream(filePath, FileMode.Create);
            await audiResp.Content!.CopyToAsync(fileStream);
            await fileStream.DisposeAsync();
        }

        mediaElement.Source = new Uri(filePath);
        mediaElement.Play();
        RunningTrack = SelectedTrack;
        
        Cache.AudioCache.Add(filePath);
    }

    [RelayCommand]
    private void Pause(MediaElement mediaElement)
    {
        mediaElement.Pause();
        _isRunning = false;
    }
    
    [RelayCommand]
    private void AddTrack()
    {
        var vm = App.Services.GetRequiredService<TrackAddViewModel>();
        IsAddingRunning = true;
        TrackAddViewModel = vm;
    }

    [RelayCommand]
    private async Task OnPageChanged(ObservableObject item)
    {
        if (item is IRefreshable refreshable && !_initialized.Contains(item.GetType()))
        {
            await refreshable.RefreshAsync();
            _initialized.Add(item.GetType());
        }
        
        SelectedPage = item;
    }

    public void Receive(CloseDialogRequest message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsAddingRunning = false;
            TrackAddViewModel = null;
        });
    }
}