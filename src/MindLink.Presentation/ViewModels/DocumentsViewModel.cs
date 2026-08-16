using System.Collections.ObjectModel;
using MindLink.Application.Models;

namespace MindLink.Presentation.ViewModels;

public sealed class DocumentsViewModel : ObservableObject
{
    private readonly WorkspaceDocument document;
    private DocumentSectionItemViewModel selectedSection;
    private string activeContextTab = "Referencias";
    private string saveStatus;
    private bool isDirty;
    private bool isBold;
    private bool isItalic;
    private int editorFontSize = 15;

    public DocumentsViewModel(WorkspaceSnapshot workspace)
    {
        document = workspace.ActiveDocument;
        ProjectTitle = workspace.Projects
            .FirstOrDefault(project => project.Id == workspace.ActiveProjectId)?.Title
            ?? document.Title;

        Sections = new ObservableCollection<DocumentSectionItemViewModel>(
            document.Sections
                .OrderBy(section => section.Order)
                .Select(section => new DocumentSectionItemViewModel(section)));

        selectedSection = Sections.FirstOrDefault(section => section.Id == document.SelectedSectionId)
            ?? Sections.First(section => !section.IsChapter);
        selectedSection.IsSelected = true;

        RelatedReferences = new ObservableCollection<RelatedReferenceItemViewModel>(
            document.RelatedReferences.Select(related =>
            {
                var reference = workspace.References.First(item => item.Id == related.ReferenceId);
                return new RelatedReferenceItemViewModel(reference, related.RelevancePercent);
            }));

        ConnectedNotes = new ObservableCollection<ConnectedNoteItemViewModel>(
            document.ConnectedNoteIds
                .Select(id => workspace.Notes.FirstOrDefault(note => note.Id == id))
                .Where(note => note is not null)
                .Select(note => new ConnectedNoteItemViewModel(note!)));

        Tags = new ObservableCollection<string>(
            RelatedReferences.SelectMany(reference => reference.Tags)
                .Concat(ConnectedNotes.SelectMany(note => note.Tags))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(5));

        saveStatus = document.LastSavedLabel;
        SelectSectionCommand = new RelayCommand<DocumentSectionItemViewModel>(SelectSection, section => section is { IsChapter: false });
        SelectContextTabCommand = new RelayCommand<string>(SelectContextTab);
        SaveCommand = new RelayCommand(Save);
        ToggleBoldCommand = new RelayCommand(() => IsBold = !IsBold);
        ToggleItalicCommand = new RelayCommand(() => IsItalic = !IsItalic);
        IncreaseFontSizeCommand = new RelayCommand(() => EditorFontSize = Math.Min(EditorFontSize + 1, 19));
        InsertCitationCommand = new RelayCommand<RelatedReferenceItemViewModel>(InsertCitation);
    }

    public string ProjectTitle { get; }
    public ObservableCollection<DocumentSectionItemViewModel> Sections { get; }
    public ObservableCollection<RelatedReferenceItemViewModel> RelatedReferences { get; }
    public ObservableCollection<ConnectedNoteItemViewModel> ConnectedNotes { get; }
    public ObservableCollection<string> Tags { get; }

    public DocumentSectionItemViewModel SelectedSection
    {
        get => selectedSection;
        private set
        {
            if (!SetProperty(ref selectedSection, value)) return;
            OnPropertyChanged(nameof(SectionContext));
            OnPropertyChanged(nameof(SectionTitle));
            OnPropertyChanged(nameof(ParagraphOne));
            OnPropertyChanged(nameof(ParagraphTwo));
            OnPropertyChanged(nameof(ParagraphThree));
            OnPropertyChanged(nameof(WordCountLabel));
            OnPropertyChanged(nameof(ShowResearchCallouts));
        }
    }

    public string SectionContext => $"CAPÍTULO {SelectedSection.ParentNumber ?? SelectedSection.Number} · SECCIÓN {SelectedSection.Number}";
    public string SectionTitle => $"{SelectedSection.Number} {SelectedSection.Title}";

    public string ParagraphOne
    {
        get => SelectedSection.ParagraphOne;
        set
        {
            if (!SelectedSection.SetParagraph(0, value)) return;
            MarkDirty();
            OnPropertyChanged();
        }
    }

    public string ParagraphTwo
    {
        get => SelectedSection.ParagraphTwo;
        set
        {
            if (!SelectedSection.SetParagraph(1, value)) return;
            MarkDirty();
            OnPropertyChanged();
        }
    }

    public string ParagraphThree
    {
        get => SelectedSection.ParagraphThree;
        set
        {
            if (!SelectedSection.SetParagraph(2, value)) return;
            MarkDirty();
            OnPropertyChanged();
        }
    }

    public string WordCountLabel => $"{SelectedSection.CalculatedWordCount:N0} palabras";
    public bool ShowResearchCallouts => false;

    public string SaveStatus
    {
        get => saveStatus;
        private set => SetProperty(ref saveStatus, value);
    }

    public bool IsDirty
    {
        get => isDirty;
        private set => SetProperty(ref isDirty, value);
    }

    public bool IsBold
    {
        get => isBold;
        private set => SetProperty(ref isBold, value);
    }

    public bool IsItalic
    {
        get => isItalic;
        private set => SetProperty(ref isItalic, value);
    }

    public int EditorFontSize
    {
        get => editorFontSize;
        private set => SetProperty(ref editorFontSize, value);
    }

    public string ActiveContextTab
    {
        get => activeContextTab;
        private set
        {
            if (!SetProperty(ref activeContextTab, value)) return;
            OnPropertyChanged(nameof(ShowingReferences));
            OnPropertyChanged(nameof(ShowingNotes));
            OnPropertyChanged(nameof(ShowingHistory));
        }
    }

    public bool ShowingReferences => ActiveContextTab == "Referencias";
    public bool ShowingNotes => ActiveContextTab == "Notas";
    public bool ShowingHistory => ActiveContextTab == "Historial";

    public RelayCommand<DocumentSectionItemViewModel> SelectSectionCommand { get; }
    public RelayCommand<string> SelectContextTabCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand ToggleBoldCommand { get; }
    public RelayCommand ToggleItalicCommand { get; }
    public RelayCommand IncreaseFontSizeCommand { get; }
    public RelayCommand<RelatedReferenceItemViewModel> InsertCitationCommand { get; }

    private void SelectSection(DocumentSectionItemViewModel? section)
    {
        if (section is null || section.IsChapter) return;
        SelectedSection.IsSelected = false;
        section.IsSelected = true;
        SelectedSection = section;
        IsDirty = false;
        SaveStatus = section.Content.Length == 0 ? "Sección lista para redactar" : "Cambios locales cargados";
    }

    private void SelectContextTab(string? tab)
    {
        if (tab is "Referencias" or "Notas" or "Historial") ActiveContextTab = tab;
    }

    private void Save()
    {
        IsDirty = false;
        SaveStatus = "Guardado localmente";
    }

    private void MarkDirty()
    {
        IsDirty = true;
        SaveStatus = "Cambios sin guardar";
        OnPropertyChanged(nameof(WordCountLabel));
    }

    private void InsertCitation(RelatedReferenceItemViewModel? reference)
    {
        reference ??= RelatedReferences.FirstOrDefault();
        if (reference is null) return;

        var suffix = $" ({reference.ShortAuthor}, {reference.Year}, p. 00)";
        ParagraphThree = string.Concat(ParagraphThree.TrimEnd(), suffix);
        SaveStatus = $"Cita de {reference.ShortAuthor} insertada";
    }
}

public sealed class DocumentSectionItemViewModel : ObservableObject
{
    private readonly List<string> paragraphs;
    private bool isSelected;

    public DocumentSectionItemViewModel(DocumentSection section)
    {
        Id = section.Id;
        ParentId = section.ParentId;
        Number = section.Number;
        ParentNumber = section.ParentId is null ? null : section.Number.Split('.')[0];
        Title = section.Title;
        Status = section.Status;
        FallbackWordCount = section.WordCount;
        paragraphs = section.Content
            .Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        while (paragraphs.Count < 3) paragraphs.Add(string.Empty);
    }

    public string Id { get; }
    public string? ParentId { get; }
    public string Number { get; }
    public string? ParentNumber { get; }
    public string Title { get; }
    public string Status { get; }
    public int FallbackWordCount { get; }
    public bool IsChapter => ParentId is null;
    public string DisplayLabel => IsChapter ? $"Capítulo {Number}: {Title}" : $"{Number} {Title}";
    public string Content => string.Join(Environment.NewLine + Environment.NewLine, paragraphs.Where(paragraph => !string.IsNullOrWhiteSpace(paragraph)));
    public string ParagraphOne => paragraphs[0];
    public string ParagraphTwo => paragraphs[1];
    public string ParagraphThree => paragraphs[2];
    public int CalculatedWordCount
    {
        get
        {
            var count = Content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
            return count == 0 ? FallbackWordCount : count;
        }
    }

    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }

    public bool SetParagraph(int index, string value)
    {
        value ??= string.Empty;
        if (paragraphs[index] == value) return false;
        paragraphs[index] = value;
        OnPropertyChanged(nameof(Content));
        OnPropertyChanged(nameof(CalculatedWordCount));
        return true;
    }
}

public sealed class RelatedReferenceItemViewModel
{
    public RelatedReferenceItemViewModel(WorkspaceReference reference, int relevance)
    {
        Id = reference.Id;
        Authors = reference.Authors;
        Year = reference.Year;
        Title = reference.Title;
        Relevance = relevance;
        Tags = reference.Tags;
        ShortAuthor = reference.Authors.Split(',')[0].Trim();
    }

    public string Id { get; }
    public string Authors { get; }
    public string ShortAuthor { get; }
    public int Year { get; }
    public string Title { get; }
    public int Relevance { get; }
    public string RelevanceLabel => $"{Relevance}%";
    public string AuthorYear => $"{Authors} ({Year})";
    public IReadOnlyList<string> Tags { get; }
}

public sealed class ConnectedNoteItemViewModel
{
    public ConnectedNoteItemViewModel(WorkspaceNote note)
    {
        Id = note.Id;
        Title = note.Title;
        Tags = note.Tags;
    }

    public string Id { get; }
    public string Title { get; }
    public IReadOnlyList<string> Tags { get; }
}
