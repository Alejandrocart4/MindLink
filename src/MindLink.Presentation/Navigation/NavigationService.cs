using Microsoft.Extensions.DependencyInjection;
using MindLink.Application.Interfaces;
using MindLink.Presentation.ViewModels;

namespace MindLink.Presentation.Navigation;

public sealed class NavigationService(
    IServiceProvider services,
    IWorkspaceDataService workspaceDataService)
{
    public event Action<ObservableObject>? RootNavigated;
    public event Action<WorkspaceRoute>? WorkspaceRouteRequested;

    public void ShowWelcome() =>
        RootNavigated?.Invoke(services.GetRequiredService<WelcomeViewModel>());

    public void ShowLogin() =>
        RootNavigated?.Invoke(services.GetRequiredService<LoginViewModel>());

    public async Task OpenWorkspaceAsync(WorkspaceRoute initialRoute = WorkspaceRoute.Dashboard)
    {
        var snapshot = await workspaceDataService.GetWorkspaceAsync(AppSession.UserId);
        var shell = services.GetRequiredService<WorkspaceShellViewModel>();
        shell.Initialize(snapshot);
        RootNavigated?.Invoke(shell);
        WorkspaceRouteRequested?.Invoke(initialRoute);
    }

    public void Navigate(WorkspaceRoute route) => WorkspaceRouteRequested?.Invoke(route);

    // Compatibilidad temporal con los ViewModels de la primera iteración.
    public void NavigateToLogin() => ShowLogin();
    public void NavigateToDashboard() => _ = OpenWorkspaceAsync();
    public void NavigateToModule(string module) => Navigate(module switch
    {
        "Notas e ideas" => WorkspaceRoute.Notes,
        "Mapa de conocimiento" => WorkspaceRoute.KnowledgeNetwork,
        "Redacción" => WorkspaceRoute.Documents,
        _ => WorkspaceRoute.Dashboard
    });
}
