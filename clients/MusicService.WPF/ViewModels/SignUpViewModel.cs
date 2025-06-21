using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MusicService.WebApi.Contracts.Requests;
using MusicService.WPF.Services;

namespace MusicService.WPF.ViewModels;

public partial class SignUpViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string? _email;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string? _username;
    
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
        
        using var response = await authEndpoints.SignUp(new SignUpRequest(Username!, Email!, Password!));

        if (!response.IsSuccessStatusCode)
        {
            var text = await response.Content.ReadAsStringAsync();
            MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var vm = scope.ServiceProvider.GetRequiredService<SignInViewModel>();
        var signInWindow = scope.ServiceProvider.GetRequiredService<SignInWindow>();

        signInWindow.DataContext = vm;
        signInWindow.Show();
        window.Close();
    }

    private bool CanConfirm() => !string.IsNullOrWhiteSpace(Email) &&
                                 !string.IsNullOrWhiteSpace(Password) &&
                                 !string.IsNullOrWhiteSpace(Username);
}