using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MindLink.Application.Interfaces;
using MindLink.Infrastructure.Persistence;
using MindLink.Infrastructure.Services;
using MindLink.Presentation.Navigation;
using MindLink.Presentation.Services;
using MindLink.Presentation.ViewModels;

namespace MindLink.Presentation;

public partial class App : System.Windows.Application
{
    private ServiceProvider? services;
    private string logPath = string.Empty;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var localDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MindLink");
        Directory.CreateDirectory(localDirectory);
        logPath = Path.Combine(localDirectory, "mindlink.log");

        DispatcherUnhandledException += (_, args) =>
        {
            LogException(args.Exception);
            MessageBox.Show(
                "MindLink encontró un problema inesperado, pero mantuvo abierto tu espacio. " +
                $"Detalle: {args.Exception.Message}",
                "MindLink",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };

        var collection = new ServiceCollection();
        var databasePath = Path.Combine(localDirectory, "mindlink.db");

        collection.AddLogging(builder => builder.AddDebug());
        collection.AddDbContext<MindLinkDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        collection.AddScoped<DatabaseInitializer>();
        collection.AddScoped<IAuthenticationService, AuthenticationService>();
        collection.AddScoped<IDashboardService, DashboardService>();
        collection.AddSingleton<IWorkspaceDataService, DemoWorkspaceDataService>();
        collection.AddSingleton<IFileDialogService, FileDialogService>();
        collection.AddSingleton<NavigationService>();
        collection.AddSingleton<WorkspaceShellViewModel>();
        collection.AddTransient<WelcomeViewModel>();
        collection.AddSingleton<MainViewModel>();

        services = collection.BuildServiceProvider();
        LogEvent("Servicios de MindLink preparados.");

        try
        {
            LogEvent("Inicializando base de datos local.");
            using var scope = services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
            LogEvent("Base de datos local lista.");
        }
        catch (Exception exception)
        {
            LogException(exception);
        }

        LogEvent("Creando ventana principal.");
        var window = new MainWindow
        {
            DataContext = services.GetRequiredService<MainViewModel>()
        };
        window.Show();
        LogEvent("Ventana principal visible.");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        services?.Dispose();
        base.OnExit(e);
    }

    private void LogException(Exception exception)
    {
        if (string.IsNullOrWhiteSpace(logPath)) return;
        File.AppendAllText(
            logPath,
            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {exception}\n\n");
    }

    private void LogEvent(string message)
    {
        if (string.IsNullOrWhiteSpace(logPath)) return;
        File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n");
    }
}
