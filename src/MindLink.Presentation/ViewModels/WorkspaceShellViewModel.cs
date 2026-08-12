using System.Collections.ObjectModel;
using MindLink.Application.Models;
using MindLink.Presentation.Navigation;

namespace MindLink.Presentation.ViewModels;

public sealed class WorkspaceShellViewModel : ObservableObject
{
    private readonly NavigationService navigation;
    private readonly Dictionary<WorkspaceRoute, ObservableObject> pages = [];
    private ObservableObject? currentPage;
    private WorkspaceSnapshot? workspace;
    private bool isSidebarExpanded = true;
    private bool isNotificationsOpen;
    private bool isAccountMenuOpen;
    private string searchText = string.Empty;
    private string statusMessage = "Guardado";

    public WorkspaceShellViewModel(NavigationService navigation)
    {
        this.navigation = navigation;
        navigation.WorkspaceRouteRequested += NavigateTo;

        NavigationItems =
        [
            new("Inicio", "\uE80F", WorkspaceRoute.Dashboard, navigation.Navigate),
            new("Proyectos", "\uE8B7", WorkspaceRoute.Projects, navigation.Navigate),
            new("Notas", "\uE70B", WorkspaceRoute.Notes, navigation.Navigate),
            new("Red de conocimiento", "\uE968", WorkspaceRoute.KnowledgeNetwork, navigation.Navigate),
            new("Referencias", "\uE82D", WorkspaceRoute.References, navigation.Navigate),
            new("Citas", "\uE8B2", WorkspaceRoute.Citations, navigation.Navigate),
            new("Documentos", "\uE8A5", WorkspaceRoute.Documents, navigation.Navigate),
            new("Historial", "\uE81C", WorkspaceRoute.History, navigation.Navigate),
            new("Exportaciones", "\uE72D", WorkspaceRoute.Export, navigation.Navigate),
            new("Configuración", "\uE713", WorkspaceRoute.Settings, navigation.Navigate)
        ];

        ToggleSidebarCommand = new RelayCommand(() => IsSidebarExpanded = !IsSidebarExpanded);
        NewNoteCommand = new RelayCommand(() => navigation.Navigate(WorkspaceRoute.Notes));
        SearchCommand = new RelayCommand(ExecuteSearch, () => !string.IsNullOrWhiteSpace(SearchText));
        ToggleNotificationsCommand = new RelayCommand(() => IsNotificationsOpen = !IsNotificationsOpen);
        CloseNotificationsCommand = new RelayCommand(() => IsNotificationsOpen = false);
        ToggleAccountMenuCommand = new RelayCommand(() => IsAccountMenuOpen = !IsAccountMenuOpen);
        CloseAccountMenuCommand = new RelayCommand(() => IsAccountMenuOpen = false);
        SignOutCommand = new RelayCommand(SignOut);
    }

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }
    public RelayCommand ToggleSidebarCommand { get; }
    public RelayCommand NewNoteCommand { get; }
    public RelayCommand SearchCommand { get; }
    public RelayCommand ToggleNotificationsCommand { get; }
    public RelayCommand CloseNotificationsCommand { get; }
    public RelayCommand ToggleAccountMenuCommand { get; }
    public RelayCommand CloseAccountMenuCommand { get; }
    public RelayCommand SignOutCommand { get; }

    public ObservableObject? CurrentPage
    {
        get => currentPage;
        private set => SetProperty(ref currentPage, value);
    }

    public WorkspaceSnapshot? Workspace
    {
        get => workspace;
        private set
        {
            if (!SetProperty(ref workspace, value)) return;
            OnPropertyChanged(nameof(UserName));
            OnPropertyChanged(nameof(UserInitials));
            OnPropertyChanged(nameof(UserRole));
            OnPropertyChanged(nameof(ActiveProjectTitle));
        }
    }

    public string UserName => Workspace?.User.FullName ?? string.Empty;
    public string UserInitials => Workspace?.User.Initials ?? string.Empty;
    public string UserRole => Workspace?.User.Role ?? string.Empty;
    public string ActiveProjectTitle => Workspace?.Projects.FirstOrDefault(p => p.Id == Workspace.ActiveProjectId)?.Title ?? string.Empty;

    public string SearchText
    {
        get => searchText;
        set
        {
            if (!SetProperty(ref searchText, value)) return;
            SearchCommand.RaiseCanExecuteChanged();
        }
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }

    public bool IsSidebarExpanded
    {
        get => isSidebarExpanded;
        set
        {
            if (!SetProperty(ref isSidebarExpanded, value)) return;
            OnPropertyChanged(nameof(SidebarWidth));
        }
    }

    public double SidebarWidth => IsSidebarExpanded ? 228 : 60;

    public bool IsNotificationsOpen
    {
        get => isNotificationsOpen;
        set => SetProperty(ref isNotificationsOpen, value);
    }

    public bool IsAccountMenuOpen
    {
        get => isAccountMenuOpen;
        set => SetProperty(ref isAccountMenuOpen, value);
    }

    public void Initialize(WorkspaceSnapshot snapshot)
    {
        Workspace = snapshot;
        pages.Clear();
        NavigationItems.Clear();
        AddNavigationItem("Inicio", "\uE80F", WorkspaceRoute.Dashboard);
        AddNavigationItem("Proyectos", "\uE8B7", WorkspaceRoute.Projects);
        AddNavigationItem("Notas", "\uE70B", WorkspaceRoute.Notes);
        AddNavigationItem("Referencias", "\uE82D", WorkspaceRoute.References);
        pages[WorkspaceRoute.Dashboard] = new DashboardViewModel(snapshot, navigation);
        pages[WorkspaceRoute.Projects] = new ProjectsViewModel(snapshot);
        pages[WorkspaceRoute.Notes] = new NotesViewModel(snapshot);
        pages[WorkspaceRoute.References] = new ReferencesViewModel(snapshot);
        if (snapshot.User.Role == "Plan Pro")
        {
            AddNavigationItem("Red de conocimiento", "\uE968", WorkspaceRoute.KnowledgeNetwork);
            AddNavigationItem("Citas", "\uE8B2", WorkspaceRoute.Citations);
            AddNavigationItem("Documentos", "\uE8A5", WorkspaceRoute.Documents);
            AddNavigationItem("Historial", "\uE81C", WorkspaceRoute.History);
            AddNavigationItem("Exportaciones", "\uE72D", WorkspaceRoute.Export);
            pages[WorkspaceRoute.KnowledgeNetwork] = new KnowledgeNetworkViewModel(snapshot);
            pages[WorkspaceRoute.Citations] = new CitationsViewModel(snapshot);
            pages[WorkspaceRoute.Documents] = new DocumentsViewModel(snapshot);
            pages[WorkspaceRoute.History] = new HistoryViewModel(snapshot);
            pages[WorkspaceRoute.Export] = new ExportViewModel(snapshot);
        }
        AddNavigationItem("Configuracion", "\uE713", WorkspaceRoute.Settings);
        pages[WorkspaceRoute.Settings] = new SettingsViewModel(snapshot);
    }

    private void AddNavigationItem(string label, string iconGlyph, WorkspaceRoute route) =>
        NavigationItems.Add(new NavigationItemViewModel(label, iconGlyph, route, navigation.Navigate));

    private void NavigateTo(WorkspaceRoute route)
    {
        if (!pages.TryGetValue(route, out var page)) return;
        CurrentPage = page;
        foreach (var item in NavigationItems) item.IsSelected = item.Route == route;
        IsNotificationsOpen = false;
        IsAccountMenuOpen = false;
    }

    private void ExecuteSearch()
    {
        if (pages.TryGetValue(WorkspaceRoute.Notes, out var page) && page is NotesViewModel notes)
            notes.SearchText = SearchText.Trim();

        StatusMessage = "Búsqueda aplicada";
        navigation.Navigate(WorkspaceRoute.Notes);
    }

    private void SignOut()
    {
        AppSession.UserId = 0;
        AppSession.UserName = string.Empty;
        AppSession.Plan = string.Empty;
        IsAccountMenuOpen = false;
        Workspace = null;
        CurrentPage = null;
        navigation.ShowLogin();
    }
}
