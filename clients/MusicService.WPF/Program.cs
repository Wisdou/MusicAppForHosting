using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MusicService.WPF.Services;
using MusicService.WPF.ViewModels;
using Refit;
using Serilog;

namespace MusicService.WPF;

public class Program
{
    private static bool IsInDebug { get; set; }

    private static Mutex? _mutex;

    [STAThread]
    public static void Main(string[] args)
    {

#if DEBUG
        IsInDebug = true;
#endif

        Log.Logger = GetLoggerConfiguration().CreateLogger();

        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", IsInDebug ? "Development" : "Production");

        _mutex = new Mutex(true, App.Title, out var createdNew);
        if (!createdNew)
        {
            MessageBox.Show("Приложение уже запущено.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            App app = new();
            app.InitializeComponent();
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Возникло исключение во время работы приложения.");
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((appConfig, _) =>
            {
                appConfig.HostingEnvironment.ApplicationName = App.Title;
                appConfig.HostingEnvironment.ContentRootPath = Environment.CurrentDirectory;
            })
            .UseSerilog()
            .ConfigureServices(ConfigureServices);
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
    {
        collection.AddSingleton<MainWindowViewModel>();
        collection.AddSingleton<MainWindow>();

        collection.AddTransient<SignInWindow>();
        collection.AddTransient<SignInViewModel>();
        
        collection.AddTransient<SignUpWindow>();
        collection.AddTransient<SignUpViewModel>();

        collection.AddTransient<TrackAddViewModel>();
        
        collection.AddSingleton<TracksPanelViewModel>();
        collection.AddSingleton<FavoritesPanelViewModel>();

        var apiBaseUrl = context.Configuration["ApiBaseUrl"];
        ArgumentException.ThrowIfNullOrWhiteSpace(apiBaseUrl);

        collection
            .AddRefitClient<IAuthEndpoints>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(apiBaseUrl);
                c.DefaultRequestHeaders.Add("Accept", "*/*");
            });
        
        collection
            .AddRefitClient<ITracksEndpoints>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(apiBaseUrl);
                c.DefaultRequestHeaders.Add("Accept", "*/*");
            });
    }

    private static LoggerConfiguration GetLoggerConfiguration()
    {
        var loggerConfiguration = new LoggerConfiguration();
        if (IsInDebug)
        {
            loggerConfiguration.MinimumLevel.Debug();
            loggerConfiguration.WriteTo.Debug();
            return loggerConfiguration;
        }

        var logsDirectory = Path.Combine(Environment.CurrentDirectory, "logs");
        if (!Directory.Exists(logsDirectory))
        {
            Directory.CreateDirectory(logsDirectory);
        }

        var logFileFullPath = Path.Combine(logsDirectory, "log.txt");

        loggerConfiguration.MinimumLevel.Information();
        loggerConfiguration.WriteTo.File(logFileFullPath, rollingInterval: RollingInterval.Day);
        return loggerConfiguration;
    }
}