using System.Collections.ObjectModel;
using MindLink.Application.Models;
using MindLink.Presentation.Services;

namespace MindLink.Presentation.ViewModels;

public sealed class NotesViewModel : ObservableObject
{
    private readonly ObservableCollection<WorkspaceNote> notes;
    private readonly WorkspaceSession session;
    private string searchText = string.Empty;
    private string currentFilter = "Todas";
    private bool isCardView = true;
    private WorkspaceNote? selectedNote;
    private string statusMessage = string.Empty;
    private string noteTitle = string.Empty;
    private string noteContent = string.Empty;
    private string noteTags = string.Empty;
    private string noteStatus = "Borrador";

    public NotesViewModel(WorkspaceSnapshot workspace, WorkspaceSession session)
    {
        notes = new ObservableCollection<WorkspaceNote>(workspace.Notes);
        this.session = session;
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
        SaveNoteCommand = new RelayCommand(SaveNote, () => SelectedNote is not null && !string.IsNullOrWhiteSpace(NoteTitle));

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
    public RelayCommand SaveNoteCommand { get; }

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
            if (!SetProperty(ref selectedNote, value)) return;
            OnPropertyChanged(nameof(HasSelectedNote));
            if (value is null)
            {
                ClearEditor();
                SaveNoteCommand.RaiseCanExecuteChanged();
                return;
            }
            NoteTitle = value.Title;
            NoteContent = value.Excerpt;
            NoteTags = string.Join(", ", value.Tags);
            NoteStatus = value.Status;
            StatusMessage = $"Nota seleccionada: {value.Title}";
            SaveNoteCommand.RaiseCanExecuteChanged();
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
    public bool HasSelectedNote => SelectedNote is not null;

    public string NoteTitle
    {
        get => noteTitle;
        set
        {
            if (SetProperty(ref noteTitle, value)) SaveNoteCommand.RaiseCanExecuteChanged();
        }
    }

    public string NoteContent { get => noteContent; set => SetProperty(ref noteContent, value); }
    public string NoteTags { get => noteTags; set => SetProperty(ref noteTags, value); }
    public string NoteStatus { get => noteStatus; set => SetProperty(ref noteStatus, value); }
    public IReadOnlyList<string> NoteStatuses { get; } = ["Borrador", "En proceso", "Revisada", "Descartada"];
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
            Title: "Nueva nota",
            Excerpt: string.Empty,
            Status: "Borrador",
            ReferencesCount: 0,
            ConnectionsCount: 0,
            UpdatedLabel: "Ahora",
            Tags: []);

        notes.Insert(0, note);
        SearchText = string.Empty;
        SetFilter("Todas");
        SelectedNote = note;
        StatusMessage = "Nueva nota creada. Escribe el título y el contenido, luego pulsa Guardar nota.";
        OnPropertyChanged(nameof(TotalNotes));
        OnPropertyChanged(nameof(ReviewedNotes));
        OnPropertyChanged(nameof(Summary));
    }

    private void SelectNote(WorkspaceNote? note)
    {
        if (note is not null) SelectedNote = note;
    }

    public void ClearSelectedNote()
    {
        SelectedNote = null;
        StatusMessage = string.Empty;
    }

    public void ChangeNoteStatus(WorkspaceNote note, string status)
    {
        if (string.IsNullOrWhiteSpace(status) || !NoteStatuses.Contains(status) || !notes.Contains(note)) return;
        var updated = note with { Status = status, UpdatedLabel = "Ahora" };
        notes[notes.IndexOf(note)] = updated;
        if (SelectedNote?.Id == note.Id) SelectedNote = updated;
        session.Update(workspace => workspace with { Notes = notes.ToArray() });
        ApplyFilters();
        StatusMessage = $"Estado actualizado a {status}.";
    }

    private void ClearEditor()
    {
        NoteTitle = string.Empty;
        NoteContent = string.Empty;
        NoteTags = string.Empty;
        NoteStatus = "Borrador";
    }

    private void SaveNote()
    {
        if (SelectedNote is null || string.IsNullOrWhiteSpace(NoteTitle)) return;
        var tags = NoteTags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var updated = SelectedNote with
        {
            Title = NoteTitle.Trim(),
            Excerpt = NoteContent.Trim(),
            Tags = tags,
            Status = NoteStatus,
            UpdatedLabel = "Ahora"
        };
        var index = notes.IndexOf(SelectedNote);
        if (index >= 0) notes[index] = updated;
        SelectedNote = updated;
        session.Update(workspace => workspace with { Notes = notes.ToArray() });
        ApplyFilters();
        StatusMessage = "Nota guardada localmente.";
        SelectedNote = null;
        OnPropertyChanged(nameof(TotalNotes));
        OnPropertyChanged(nameof(ReviewedNotes));
        OnPropertyChanged(nameof(Summary));
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
