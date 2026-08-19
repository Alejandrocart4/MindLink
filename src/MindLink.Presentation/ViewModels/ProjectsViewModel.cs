using System.Collections.ObjectModel;
using MindLink.Application.Models;
using MindLink.Presentation.Services;

namespace MindLink.Presentation.ViewModels;

public sealed class ProjectsViewModel : ObservableObject
{
    private readonly ObservableCollection<WorkspaceProject> projects;
    private readonly WorkspaceSession session;
    private string searchText = string.Empty;
    private string currentFilter = "Todos";
    private bool isCardView = true;
    private WorkspaceProject? selectedProject;
    private string statusMessage = string.Empty;

    public ProjectsViewModel(WorkspaceSnapshot workspace, WorkspaceSession session)
    {
        projects = new ObservableCollection<WorkspaceProject>(workspace.Projects);
        this.session = session;
        VisibleProjects = [];
        Filters =
        [
            new ProjectFilterOption("Todos", true),
            new ProjectFilterOption("Idea"),
            new ProjectFilterOption("Investigación"),
            new ProjectFilterOption("Redacción"),
            new ProjectFilterOption("Revisión"),
            new ProjectFilterOption("Finalizado")
        ];

        SetFilterCommand = new RelayCommand<string>(SetFilter);
        ShowCardsCommand = new RelayCommand(() => IsCardView = true);
        ShowListCommand = new RelayCommand(() => IsCardView = false);
        CreateProjectCommand = new RelayCommand(CreateProject);
        SelectProjectCommand = new RelayCommand<WorkspaceProject>(SelectProject);

        ApplyFilters();
    }

    public ObservableCollection<WorkspaceProject> VisibleProjects { get; }
    public ObservableCollection<ProjectFilterOption> Filters { get; }
    public RelayCommand<string> SetFilterCommand { get; }
    public RelayCommand ShowCardsCommand { get; }
    public RelayCommand ShowListCommand { get; }
    public RelayCommand CreateProjectCommand { get; }
    public RelayCommand<WorkspaceProject> SelectProjectCommand { get; }

    public string SearchText
    {
        get => searchText;
        set
        {
            if (!SetProperty(ref searchText, value)) return;
            ApplyFilters();
        }
    }

    public string CurrentFilter
    {
        get => currentFilter;
        private set => SetProperty(ref currentFilter, value);
    }

    public bool IsCardView
    {
        get => isCardView;
        set
        {
            if (!SetProperty(ref isCardView, value)) return;
            OnPropertyChanged(nameof(IsListView));
        }
    }

    public bool IsListView => !IsCardView;

    public WorkspaceProject? SelectedProject
    {
        get => selectedProject;
        set
        {
            if (!SetProperty(ref selectedProject, value) || value is null) return;
            StatusMessage = $"{value.ShortTitle} está seleccionado.";
        }
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set
        {
            if (!SetProperty(ref statusMessage, value)) return;
            OnPropertyChanged(nameof(HasStatusMessage));
        }
    }

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);
    public int TotalProjects => projects.Count;
    public int ActiveProjects => projects.Count(project => project.Status != "Finalizado");
    public string Summary => $"{TotalProjects} proyectos · {ActiveProjects} en progreso";

    private void SetFilter(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return;
        CurrentFilter = filter;
        foreach (var option in Filters)
        {
            option.IsSelected = option.Label == filter;
        }

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var query = projects.Where(project =>
            (CurrentFilter == "Todos" || project.Status == CurrentFilter) &&
            (string.IsNullOrWhiteSpace(SearchText) ||
             project.Title.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase) ||
             project.Description.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase) ||
             project.Tags.Any(tag => tag.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase))));

        VisibleProjects.Clear();
        foreach (var project in query)
        {
            VisibleProjects.Add(project);
        }

        OnPropertyChanged(nameof(VisibleProjects));
    }

    private void CreateProject()
    {
        var project = new WorkspaceProject(
            Id: $"project-local-{Guid.NewGuid():N}",
            Title: "Alfabetización en inteligencia artificial para docentes universitarios",
            ShortTitle: "Alfabetización en IA docente",
            Description: "Proyecto local recién creado para organizar preguntas, fuentes y primeras hipótesis de investigación.",
            Status: "Idea",
            Progress: 6,
            NotesCount: 1,
            ReferencesCount: 0,
            UpdatedLabel: "Ahora",
            Tags: ["Inteligencia artificial", "Formación docente"],
            CollaboratorInitials: ["MR"]);

        projects.Insert(0, project);
        SearchText = string.Empty;
        SetFilter("Todos");
        SelectedProject = project;
        session.Update(workspace => workspace with { Projects = projects.ToArray(), ActiveProjectId = project.Id });
        StatusMessage = "Nuevo proyecto creado en este espacio local.";
        OnPropertyChanged(nameof(TotalProjects));
        OnPropertyChanged(nameof(ActiveProjects));
        OnPropertyChanged(nameof(Summary));
    }

    private void SelectProject(WorkspaceProject? project)
    {
        if (project is not null) SelectedProject = project;
    }
}

public sealed class ProjectFilterOption : ObservableObject
{
    private bool isSelected;

    public ProjectFilterOption(string label, bool isSelected = false)
    {
        Label = label;
        this.isSelected = isSelected;
    }

    public string Label { get; }

    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }
}
