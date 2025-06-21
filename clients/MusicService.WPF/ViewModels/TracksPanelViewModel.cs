using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using MusicService.WebApi.Contracts.Responses;
using MusicService.WPF.Services;
using Serilog;

namespace MusicService.WPF.ViewModels;

public static class Cache
{
    public static readonly ConcurrentDictionary<Guid, BitmapImage> ImageCache = new();

    public static readonly ConcurrentBag<string> AudioCache = new();
}

public interface IRefreshable
{
    Task RefreshAsync();
}

public partial class TracksPanelViewModel : ObservableRecipient, IRefreshable, IRecipient<TrackAddedMessage>
{
    [ObservableProperty] 
    private IEnumerable<TrackViewModel>? _tracks;

    [RelayCommand]
    public async Task RefreshAsync()
    {
        using var scope = App.Services.CreateScope();
        var endpoints = scope.ServiceProvider.GetRequiredService<ITracksEndpoints>();

        using var response = await endpoints.GetTracksAsync(App.AccessToken);

        Application.Current.Dispatcher.Invoke(() =>
        {
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Tracks = response.Content.Select(e => new TrackViewModel { Track = e }).ToList();
            }
        });
        
        await Task.Run(async () =>
        {
            foreach (var track in Tracks ?? [])
            {
                if (Cache.ImageCache.TryGetValue(track.Track.Id, out var image))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        track.Image = image;
                    });
                    
                    continue;
                }
                
                try
                {
                    var newEnd = scope.ServiceProvider.GetRequiredService<ITracksEndpoints>();
                    using var coverResp = await newEnd.LoadCover(App.AccessToken, track.Track.Id);

                    if (coverResp.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        continue;
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = coverResp.Content;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();

                        Cache.ImageCache[track.Track.Id] = bitmapImage;
                        track.Image = bitmapImage;
                    });
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to load cover");
                }
            }
        });
    }

    public async void Receive(TrackAddedMessage message)
    {
        await RefreshAsync();
    }
}

public partial class TrackViewModel : ObservableObject
{
    public required TrackResponse Track { get; init; }

    [ObservableProperty] private BitmapImage? _image;

    public bool IsNotInFavorites => !Track.IsInFavorites;
}