using System.Collections.ObjectModel;
using MindLink.Application.Models;

namespace MindLink.Presentation.ViewModels;

public sealed class NotesViewModel : ObservableObject
{
    private readonly ObservableCollection<WorkspaceNote> notes;
    private string searchText = string.Empty;
    private string currentFilter = "Todas";
    private bool isCardView = true;
    private WorkspaceNote? selectedNote;
    private string statusMessage = string.Empty;

    public NotesViewModel(WorkspaceSnapshot workspace)
    {
        notes = new ObservableCollection<WorkspaceNote>(workspace.Notes);
        ActiveProjectId = workspace.ActiveProjectId;
        VisibleNotes = [];
        Filters =
        [
            new NoteFilterOption("Todas", true),
            new NoteFilterOption("Revisada"),
            new NoteFilterOption("Borrador")
        ];

        SetFilterCommand = new RelayCommand<string>(SetFilter);
        ShowCardsCommand = new RelayCommand(() => IsCardView = true);
        ShowListCommand = new RelayCommand(() => IsCardView = false);
        CreateNoteCommand = new RelayCommand(CreateNote);
        SelectNoteCommand = new RelayCommand<WorkspaceNote>(SelectNote);

        ApplyFilters();
    }

    public string ActiveProjectId { get; }
    public ObservableCollection<WorkspaceNote> VisibleNotes { get; }
    public ObservableCollection<NoteFilterOption> Filters { get; }
    public RelayCommand<string> SetFilterCommand { get; }
    public RelayCommand ShowCardsCommand { get; }
    public RelayCommand ShowListCommand { get; }
    public RelayCommand CreateNoteCommand { get; }
    public RelayCommand<WorkspaceNote> SelectNoteCommand { get; }

    public string SearchText
    {
        get => searchText;
        set
        {
            if (!SetProperty(ref searchText, value)) return;
            ApplyFilters();
        }
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

    public WorkspaceNote? SelectedNote
    {
        get => selectedNote;
        set
        {
            if (!SetProperty(ref selectedNote, value) || value is null) return;
            StatusMessage = $"Nota seleccionada: {value.Title}";
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
    public int TotalNotes => notes.Count;
    public int ReviewedNotes => notes.Count(note => note.Status == "Revisada");
    public string Summary => $"{TotalNotes} notas · {ReviewedNotes} revisadas";

    private void SetFilter(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return;
        currentFilter = filter;
        foreach (var option in Filters)
        {
            option.IsSelected = option.Label == filter;
        }

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var query = notes.Where(note =>
            (currentFilter == "Todas" || note.Status == currentFilter) &&
            (string.IsNullOrWhiteSpace(SearchText) ||
             note.Title.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase) ||
             note.Excerpt.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase) ||
             note.Tags.Any(tag => tag.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase))));

        VisibleNotes.Clear();
        foreach (var note in query)
        {
            VisibleNotes.Add(note);
        }

        OnPropertyChanged(nameof(VisibleNotes));
    }

    private void CreateNote()
    {
        var note = new WorkspaceNote(
            Id: $"note-local-{Guid.NewGuid():N}",
            ProjectId: ActiveProjectId,
            Title: "Preguntas para la siguiente revisión de literatura",
            Excerpt: "¿Qué evidencia reciente explica cuándo la personalización con IA mejora el aprendizaje y cuándo aumenta la carga cognitiva?",
            Status: "Borrador",
            ReferencesCount: 0,
            ConnectionsCount: 1,
            UpdatedLabel: "Ahora",
            Tags: ["Preguntas", "Revisión", "IA"]);

        notes.Insert(0, note);
        SearchText = string.Empty;
        SetFilter("Todas");
        SelectedNote = note;
        StatusMessage = "Nueva nota creada y lista para editar.";
        OnPropertyChanged(nameof(TotalNotes));
        OnPropertyChanged(nameof(ReviewedNotes));
        OnPropertyChanged(nameof(Summary));
    }

    private void SelectNote(WorkspaceNote? note)
    {
        if (note is not null) SelectedNote = note;
    }
}

public sealed class NoteFilterOption : ObservableObject
{
    private bool isSelected;

    public NoteFilterOption(string label, bool isSelected = false)
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
