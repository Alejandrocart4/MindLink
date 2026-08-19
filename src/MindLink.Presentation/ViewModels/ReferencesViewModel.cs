using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using MindLink.Application.Models;
using MindLink.Presentation.Services;

namespace MindLink.Presentation.ViewModels;

public sealed class ReferencesViewModel : ObservableObject
{
    private readonly ObservableCollection<WorkspaceReference> references;
    private readonly WorkspaceSession session;
    private string searchText = string.Empty;
    private string currentFilter = "Todas";
    private string selectedFormat = "APA";
    private bool isTableView = true;
    private bool areFiltersVisible;
    private WorkspaceReference? selectedReference;
    private string statusMessage = string.Empty;

    public ReferencesViewModel(WorkspaceSnapshot workspace, WorkspaceSession session)
    {
        references = new ObservableCollection<WorkspaceReference>(workspace.References);
        this.session = session;
        ActiveProjectId = workspace.ActiveProjectId;
        VisibleReferences = [];
        Filters =
        [
            new ReferenceFilterOption("Todas", true),
            new ReferenceFilterOption("Leído"),
            new ReferenceFilterOption("En lectura"),
            new ReferenceFilterOption("Por leer"),
            new ReferenceFilterOption("Incompletas")
        ];
        Formats =
        [
            new CitationFormatOption("APA", true),
            new CitationFormatOption("MLA"),
            new CitationFormatOption("Chicago")
        ];

        SetFilterCommand = new RelayCommand<string>(SetFilter);
        SetFormatCommand = new RelayCommand<string>(SetFormat);
        ToggleFiltersCommand = new RelayCommand(() => AreFiltersVisible = !AreFiltersVisible);
        ShowTableCommand = new RelayCommand(() => IsTableView = true);
        ShowCardsCommand = new RelayCommand(() => IsTableView = false);
        CreateReferenceCommand = new RelayCommand(CreateReference);
        ImportReferenceCommand = new RelayCommand(ImportReference);
        SelectReferenceCommand = new RelayCommand<WorkspaceReference>(SelectReference);
        CopyCitationCommand = new RelayCommand<WorkspaceReference>(CopyCitation);
        OpenExternalCommand = new RelayCommand<WorkspaceReference>(OpenExternal);
        InsertCitationCommand = new RelayCommand(InsertCitation, () => SelectedReference is not null);

        ApplyFilters();
    }

    public string ActiveProjectId { get; }
    public ObservableCollection<WorkspaceReference> VisibleReferences { get; }
    public ObservableCollection<ReferenceFilterOption> Filters { get; }
    public ObservableCollection<CitationFormatOption> Formats { get; }
    public RelayCommand<string> SetFilterCommand { get; }
    public RelayCommand<string> SetFormatCommand { get; }
    public RelayCommand ToggleFiltersCommand { get; }
    public RelayCommand ShowTableCommand { get; }
    public RelayCommand ShowCardsCommand { get; }
    public RelayCommand CreateReferenceCommand { get; }
    public RelayCommand ImportReferenceCommand { get; }
    public RelayCommand<WorkspaceReference> SelectReferenceCommand { get; }
    public RelayCommand<WorkspaceReference> CopyCitationCommand { get; }
    public RelayCommand<WorkspaceReference> OpenExternalCommand { get; }
    public RelayCommand InsertCitationCommand { get; }

    public string SearchText
    {
        get => searchText;
        set
        {
            if (!SetProperty(ref searchText, value)) return;
            ApplyFilters();
        }
    }

    public bool IsTableView
    {
        get => isTableView;
        set
        {
            if (!SetProperty(ref isTableView, value)) return;
            OnPropertyChanged(nameof(IsCardView));
        }
    }

    public bool IsCardView => !IsTableView;

    public bool AreFiltersVisible
    {
        get => areFiltersVisible;
        set => SetProperty(ref areFiltersVisible, value);
    }

    public WorkspaceReference? SelectedReference
    {
        get => selectedReference;
        set
        {
            if (!SetProperty(ref selectedReference, value)) return;
            OnPropertyChanged(nameof(HasSelectedReference));
            OnPropertyChanged(nameof(SelectedCitation));
            InsertCitationCommand.RaiseCanExecuteChanged();
        }
    }

    public bool HasSelectedReference => SelectedReference is not null;

    public string SelectedFormat
    {
        get => selectedFormat;
        private set
        {
            if (!SetProperty(ref selectedFormat, value)) return;
            OnPropertyChanged(nameof(SelectedCitation));
        }
    }

    public string SelectedCitation => SelectedReference is null
        ? string.Empty
        : FormatCitation(SelectedReference, SelectedFormat);

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
    public int TotalReferences => references.Count;
    public int ReadReferences => references.Count(reference => reference.ReadingStatus == "Leído");
    public int IncompleteReferences => references.Count(reference => !reference.IsComplete);
    public int UnclassifiedReferences => references.Count(reference => !reference.IsClassified);
    public string IncompleteReferencesLabel => $"{IncompleteReferences} {(IncompleteReferences == 1 ? "referencia incompleta" : "referencias incompletas")} (faltan campos requeridos)";
    public string UnclassifiedReferencesLabel => $"{UnclassifiedReferences} {(UnclassifiedReferences == 1 ? "referencia sin clasificar" : "referencias sin clasificar")} por proyecto";
    public string Summary => $"{TotalReferences} referencias · {ReadReferences} leídas · {IncompleteReferences} incompletas";

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

    private void SetFormat(string? format)
    {
        if (string.IsNullOrWhiteSpace(format)) return;
        SelectedFormat = format;
        foreach (var option in Formats)
        {
            option.IsSelected = option.Label == format;
        }
    }

    private void ApplyFilters()
    {
        var query = references.Where(reference =>
            MatchesFilter(reference) &&
            (string.IsNullOrWhiteSpace(SearchText) ||
             reference.Authors.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase) ||
             reference.Title.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase) ||
             reference.Identifier.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase) ||
             reference.Tags.Any(tag => tag.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase))));

        VisibleReferences.Clear();
        foreach (var reference in query)
        {
            VisibleReferences.Add(reference);
        }

        OnPropertyChanged(nameof(VisibleReferences));
    }

    private bool MatchesFilter(WorkspaceReference reference) => currentFilter switch
    {
        "Todas" => true,
        "Incompletas" => !reference.IsComplete,
        _ => reference.ReadingStatus == currentFilter
    };

    private void CreateReference()
    {
        var reference = new WorkspaceReference(
            Id: $"reference-local-{Guid.NewGuid():N}",
            ProjectId: ActiveProjectId,
            Authors: "Williamson, B. & Eynon, R.",
            Year: 2020,
            Title: "Historical threads, missing links, and future directions in AI in education",
            Source: "Learning, Media and Technology",
            ReferenceType: "Artículo científico",
            ReadingStatus: "Por leer",
            UsageCount: 0,
            IsComplete: true,
            IsClassified: true,
            Identifier: "https://doi.org/10.1080/17439884.2020.1798995",
            Tags: ["Historia", "IA educativa", "Investigación"]);

        AddReference(reference, "Nueva referencia agregada al proyecto activo.");
    }

    private void ImportReference()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Importar referencia",
            Filter = "Referencias RIS (*.ris)|*.ris|Archivos BibTeX (*.bib)|*.bib|Todos los archivos (*.*)|*.*"
        };
        if (dialog.ShowDialog() != true) return;

        string[] lines;
        try { lines = File.ReadAllLines(dialog.FileName); }
        catch (Exception exception)
        {
            StatusMessage = $"No se pudo leer la referencia: {exception.Message}";
            return;
        }

        var authors = lines.Where(line => line.StartsWith("AU  - ", StringComparison.OrdinalIgnoreCase))
            .Select(line => line[6..].Trim()).ToArray();
        var title = ReadRisValue(lines, "TI  - ") ?? ReadRisValue(lines, "T1  - ") ?? Path.GetFileNameWithoutExtension(dialog.FileName);
        var yearText = ReadRisValue(lines, "PY  - ") ?? ReadRisValue(lines, "Y1  - ") ?? DateTime.Now.Year.ToString();
        var year = int.TryParse(yearText[..Math.Min(4, yearText.Length)], out var parsedYear) ? parsedYear : DateTime.Now.Year;
        var reference = new WorkspaceReference(
            Id: $"reference-imported-{Guid.NewGuid():N}",
            ProjectId: ActiveProjectId,
            Authors: authors.Length == 0 ? "Autor no especificado" : string.Join(" & ", authors),
            Year: year,
            Title: title,
            Source: ReadRisValue(lines, "JO  - ") ?? ReadRisValue(lines, "T2  - ") ?? "Fuente importada",
            ReferenceType: "Referencia importada",
            ReadingStatus: "Por leer",
            UsageCount: 0,
            IsComplete: true,
            IsClassified: true,
            Identifier: ReadRisValue(lines, "DO  - ") ?? ReadRisValue(lines, "UR  - ") ?? string.Empty,
            Tags: ["Importada"]);

        AddReference(reference, "Referencia importada y guardada correctamente.");
    }

    private static string? ReadRisValue(IEnumerable<string> lines, string prefix) => lines
        .FirstOrDefault(line => line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))?[prefix.Length..].Trim();

    private void AddReference(WorkspaceReference reference, string message)
    {
        references.Insert(0, reference);
        SearchText = string.Empty;
        SetFilter("Todas");
        SelectedReference = reference;
        session.Update(workspace => workspace with { References = references.ToArray() });
        StatusMessage = message;
        OnPropertyChanged(nameof(TotalReferences));
        OnPropertyChanged(nameof(ReadReferences));
        OnPropertyChanged(nameof(IncompleteReferences));
        OnPropertyChanged(nameof(UnclassifiedReferences));
        OnPropertyChanged(nameof(IncompleteReferencesLabel));
        OnPropertyChanged(nameof(UnclassifiedReferencesLabel));
        OnPropertyChanged(nameof(Summary));
    }

    private void SelectReference(WorkspaceReference? reference)
    {
        SelectedReference = ReferenceEquals(SelectedReference, reference) ? null : reference;
    }

    private void CopyCitation(WorkspaceReference? reference)
    {
        if (reference is null) return;
        try
        {
            Clipboard.SetText(FormatCitation(reference, SelectedFormat));
            StatusMessage = $"Cita {SelectedFormat} copiada al portapapeles.";
        }
        catch
        {
            StatusMessage = "No fue posible acceder al portapapeles. Inténtalo nuevamente.";
        }
    }

    private void OpenExternal(WorkspaceReference? reference)
    {
        if (reference is null) return;
        if (!Uri.TryCreate(reference.Identifier, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            StatusMessage = "Esta referencia no tiene un enlace externo disponible.";
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
            StatusMessage = "Abriendo la fuente en el navegador.";
        }
        catch
        {
            StatusMessage = "No se pudo abrir el enlace externo.";
        }
    }

    private void InsertCitation()
    {
        if (SelectedReference is null) return;
        var citation = $" ({SelectedReference.Authors.Split(',')[0].Trim()}, {SelectedReference.Year})";
        session.Update(workspace =>
        {
            var document = workspace.ActiveDocument;
            var sections = document.Sections.Select(section => section.Id != document.SelectedSectionId
                ? section
                : section with { Content = string.Concat(section.Content.TrimEnd(), citation), WordCount = section.WordCount + 2 }).ToArray();
            return workspace with { ActiveDocument = document with { Sections = sections, LastSavedLabel = "Cita insertada ahora" } };
        });
        StatusMessage = $"La cita de {SelectedReference.Authors} se insertó en el documento activo.";
    }

    private static string FormatCitation(WorkspaceReference reference, string format)
    {
        var identifier = string.IsNullOrWhiteSpace(reference.Identifier) ? string.Empty : $" {reference.Identifier}";
        return format switch
        {
            "MLA" => $"{reference.Authors}. \"{reference.Title}.\" {reference.Source}, {reference.Year}.{identifier}",
            "Chicago" => $"{reference.Authors}. {reference.Year}. \"{reference.Title}.\" {reference.Source}.{identifier}",
            _ => $"{reference.Authors} ({reference.Year}). {reference.Title}. {reference.Source}.{identifier}"
        };
    }
}

public sealed class ReferenceFilterOption : ObservableObject
{
    private bool isSelected;

    public ReferenceFilterOption(string label, bool isSelected = false)
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

public sealed class CitationFormatOption : ObservableObject
{
    private bool isSelected;

    public CitationFormatOption(string label, bool isSelected = false)
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
