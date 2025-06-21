using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MusicService.WebApi.Contracts.Requests;
using MusicService.WPF.Services;

namespace MusicService.WPF.ViewModels;

public partial class SignInViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string? _emailOrUsername;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string? _password;
    
    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private async Task Confirm(Window window)
    {
        if (ConfirmCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var authEndpoints = scope.ServiceProvider.GetRequiredService<IAuthEndpoints>();

        using var response = await authEndpoints.SignIn(new SignInRequest(EmailOrUsername!, Password!));

        if (!response.IsSuccessStatusCode)
        {
            var text = response.Error.Content;
            MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        App.AccessToken = response.Content!.AccessToken;
        await File.WriteAllTextAsync(App.AccessTokenFilePath, App.AccessToken);
        
        var meResponse = await authEndpoints.GetMe(App.AccessToken);
        App.CurrentUser = meResponse.Content;

        var vm = scope.ServiceProvider.GetRequiredService<MainWindowViewModel>();
        var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();

        mainWindow.DataContext = vm;
        mainWindow.Show();
        
        window.Close();
    }

    [RelayCommand]
    private static void SignUp(Window window)
    {
        using var scope = App.Services.CreateScope();
        var vm = scope.ServiceProvider.GetRequiredService<SignUpViewModel>();
        var signUpWindow = scope.ServiceProvider.GetRequiredService<SignUpWindow>();
        
        signUpWindow.DataContext = vm;
        signUpWindow.Show();
        
        window.Close();
    }

    private bool CanConfirm() => !string.IsNullOrWhiteSpace(EmailOrUsername) &&
                                 !string.IsNullOrWhiteSpace(Password);
}