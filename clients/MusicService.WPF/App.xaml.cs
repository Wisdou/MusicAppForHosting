using System.IO;
using System.Net;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using MusicService.WPF.Services;
using MusicService.WPF.ViewModels;
using Serilog;
using Path = System.IO.Path;

namespace MusicService.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public const string Title = "AnswerScanner";
    
    public static readonly string AccessTokenFilePath = Path.Combine(Environment.CurrentDirectory, "credentials.txt");

    public static string AccessToken { get; set; } = string.Empty;
    
    public static CurrentUser? CurrentUser { get; set; }
    
    public static IServiceProvider Services { get; }

    static App()
    {
        Services = Program.CreateHostBuilder(Environment.GetCommandLineArgs()).Build().Services;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        foreach (var item in Cache.AudioCache)
        {
            if (File.Exists(item))
            {
                File.Delete(item);
            }
        }
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);

            ObservableObject vm;
            Window window;
        
            if (File.Exists(AccessTokenFilePath) && (await File.ReadAllTextAsync(AccessTokenFilePath)).Trim() is { Length: > 0 } accessToken)
            {
                AccessToken = accessToken;
            
                using var scope = Services.CreateScope();
                var authEndpoints = scope.ServiceProvider.GetRequiredService<IAuthEndpoints>();

                using var response = await authEndpoints.GetMe(accessToken);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    CurrentUser = response.Content;
                
                    vm = Services.GetRequiredService<MainWindowViewModel>();
                    window = Services.GetRequiredService<MainWindow>();

                    window.DataContext = vm;
                    window.Show();
            
                    return;
                }
            }

            vm = Services.GetRequiredService<SignInViewModel>();
            window = Services.GetRequiredService<SignInWindow>();

            window.DataContext = vm;
            window.Show();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception");
        }
    }
}

public record CurrentUser(Guid Id, string Role, string Email, string Username);