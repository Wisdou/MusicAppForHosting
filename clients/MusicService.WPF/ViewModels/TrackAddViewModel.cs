using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using MusicService.Domain.Entities;
using MusicService.WPF.Services;
using Refit;

namespace MusicService.WPF.ViewModels;

public partial class TrackAddViewModel : ObservableRecipient
{
    public static readonly IEnumerable<EnumViewModel<TrackGenre>> TrackGenres = Enum
        .GetValues<TrackGenre>()
        .Select(t => new EnumViewModel<TrackGenre>(t))
        .ToList();
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddTrackCommand))]
    private string? _title;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddTrackCommand))]
    private string? _performer;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddTrackCommand))]
    private EnumViewModel<TrackGenre>? _genre;

    [ObservableProperty]
    private string? _coverPath;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddTrackCommand))]
    private string? _audioPath;
    
    [RelayCommand]
    private void SelectCover()
    {
        const string title = "Выберите файлы:";
        const string filter = "Files|*.png;*.jpg;*.jpeg";

        var fileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = title,
            RestoreDirectory = true,
            Multiselect = false,
            Filter = filter,
        };

        fileDialog.ShowDialog();
        CoverPath = fileDialog.FileName;
    }
    
    [RelayCommand]
    private void SelectAudio()
    {
        const string title = "Выберите файлы:";
        const string filter = "Files|*.mp3;";

        var fileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = title,
            RestoreDirectory = true,
            Multiselect = false,
            Filter = filter,
        };

        fileDialog.ShowDialog();
        AudioPath = fileDialog.FileName;
    }

    [RelayCommand(CanExecute = nameof(CanAddTrack))]
    private async Task AddTrack()
    {
        if (AddTrackCommand.IsRunning)
        {
            return;
        }
        
        using var scope = App.Services.CreateScope();
        var endpoints = scope.ServiceProvider.GetRequiredService<ITracksEndpoints>();
        
        string jsonPart = JsonSerializer.Serialize(new
        {
            Title = Title?.Trim(),
            Performer = Performer?.Trim(),
            Genre = Genre!.Value
        });

        var cover = string.IsNullOrWhiteSpace(CoverPath) ? null : new FileInfoPart(new FileInfo(CoverPath), "cover", MediaTypeNames.Image.Jpeg);
        var audio = new FileInfoPart(new FileInfo(AudioPath!), "audio", "audio/mpeg");
        
        using var response = await endpoints.AddTrack(App.AccessToken, cover, audio, jsonPart!);
        if (response.IsSuccessStatusCode)
        {
            Messenger.Send(new TrackAddedMessage());
            Messenger.Send(new CloseDialogRequest());
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool CanAddTrack() => !string.IsNullOrWhiteSpace(Title) &&
                                  !string.IsNullOrWhiteSpace(Performer) &&
                                  !string.IsNullOrWhiteSpace(AudioPath) &&
                                  Genre is not null;
    
    [RelayCommand]
    private void Close()
    {
        Messenger.Send(new CloseDialogRequest());
    }
}

public record CloseDialogRequest();

public record TrackAddedMessage();

public record EnumViewModel<T> where T : Enum
{
    public string? DisplayTitle { get; }
    
    public T Value { get; }
    
    public EnumViewModel(T value)
    {
        Value = value;
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DisplayAttribute>();
        DisplayTitle = attribute?.Name;
    }
}