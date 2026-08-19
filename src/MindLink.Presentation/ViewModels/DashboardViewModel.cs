using System.Collections.ObjectModel;
using MindLink.Application.Models;
using MindLink.Presentation.Navigation;
using MindLink.Presentation.Services;

namespace MindLink.Presentation.ViewModels;

public sealed class DashboardViewModel : ObservableObject
{
    private readonly NavigationService navigation;
    private readonly WorkspaceSession session;
    private string feedbackMessage = string.Empty;

    public DashboardViewModel(WorkspaceSnapshot snapshot, NavigationService navigation, WorkspaceSession session)
    {
        this.navigation = navigation;
        this.session = session;
        UserFirstName = snapshot.User.FirstName;
        Metrics = snapshot.Metrics.Select(DashboardMetricViewModel.FromModel).ToArray();
        QuickActions = snapshot.QuickActions;
        RecentProjects = snapshot.Projects.Take(3).ToArray();
        RecentActivity = snapshot.Activities.Take(5).ToArray();
        PendingTasks = new ObservableCollection<WorkspaceTask>(snapshot.PendingTasks);

        QuickActionCommand = new RelayCommand<string>(OpenQuickAction);
        OpenProjectsCommand = new RelayCommand(() => navigation.Navigate(WorkspaceRoute.Projects));
        OpenProjectCommand = new RelayCommand<WorkspaceProject>(_ => navigation.Navigate(WorkspaceRoute.Documents));
        OpenNetworkCommand = new RelayCommand(() => navigation.Navigate(WorkspaceRoute.KnowledgeNetwork));
        CompleteTaskCommand = new RelayCommand<WorkspaceTask>(CompleteTask);
    }

    public string UserFirstName { get; }
    public string Greeting => $"Buenos días, {UserFirstName} 👋";
    public IReadOnlyList<DashboardMetricViewModel> Metrics { get; }
    public IReadOnlyList<WorkspaceQuickAction> QuickActions { get; }
    public IReadOnlyList<WorkspaceProject> RecentProjects { get; }
    public IReadOnlyList<WorkspaceActivity> RecentActivity { get; }
    public ObservableCollection<WorkspaceTask> PendingTasks { get; }
    public RelayCommand<string> QuickActionCommand { get; }
    public RelayCommand OpenProjectsCommand { get; }
    public RelayCommand<WorkspaceProject> OpenProjectCommand { get; }
    public RelayCommand OpenNetworkCommand { get; }
    public RelayCommand<WorkspaceTask> CompleteTaskCommand { get; }

    public string FeedbackMessage
    {
        get => feedbackMessage;
        private set => SetProperty(ref feedbackMessage, value);
    }

    private void OpenQuickAction(string? route)
    {
        var target = route switch
        {
            string value when value.StartsWith("notes", StringComparison.OrdinalIgnoreCase) => WorkspaceRoute.Notes,
            string value when value.StartsWith("references", StringComparison.OrdinalIgnoreCase) => WorkspaceRoute.References,
            string value when value.StartsWith("knowledge", StringComparison.OrdinalIgnoreCase) => WorkspaceRoute.KnowledgeNetwork,
            string value when value.StartsWith("documents", StringComparison.OrdinalIgnoreCase) => WorkspaceRoute.Documents,
            _ => WorkspaceRoute.Dashboard
        };
        navigation.Navigate(target);
    }

    private void CompleteTask(WorkspaceTask? task)
    {
        if (task is null) return;
        PendingTasks.Remove(task);
        session.Update(workspace => workspace with { PendingTasks = workspace.PendingTasks.Where(item => item.Id != task.Id).ToArray() });
        FeedbackMessage = $"Completaste: {task.Title}";
    }
}

public sealed record DashboardMetricViewModel(
    string Label,
    int Value,
    string Detail,
    string IconGlyph,
    string AccentKey)
{
    public static DashboardMetricViewModel FromModel(WorkspaceMetric metric) => new(
        metric.Label,
        metric.Value,
        metric.Detail,
        metric.IconKey switch
        {
            "Projects" => "\uE8B7",
            "Notes" => "\uE70B",
            "References" => "\uE82D",
            _ => "\uE968"
        },
        metric.AccentKey);
}
