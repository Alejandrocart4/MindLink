using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security;
using System.Text;
using System.Windows.Input;
using Microsoft.Win32;
using MindLink.Application.Models;
using MindLink.Presentation.Services;

namespace MindLink.Presentation.ViewModels;

public sealed class ExportViewModel : ObservableObject
{
    private readonly WorkspaceSnapshot workspace;
    private readonly WorkspaceSession session;
    private int currentStep = 1;
    private ExportOptionItemViewModel? selectedFormat;
    private string citationStyle = "APA";
    private bool includeCover = true;
    private bool includeTableOfContents = true;
    private bool includeBibliography = true;
    private bool exportCompleted;
    private string fileName = "Impacto_IA_Educacion_v4";
    private string? lastExportPath;
    private string statusMessage = string.Empty;

    public ExportViewModel(WorkspaceSnapshot workspace, WorkspaceSession session)
    {
        this.workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        this.session = session;

        Formats = new ObservableCollection<ExportOptionItemViewModel>(
            workspace.ExportOptions.Select(option => new ExportOptionItemViewModel(option)));
        Sections = new ObservableCollection<ExportSectionItemViewModel>(CreateSections(workspace));
        Steps = new ObservableCollection<ExportStepItemViewModel>
        {
            new(1, "Formato"),
            new(2, "Opciones"),
            new(3, "Vista previa"),
            new(4, "Exportar")
        };

        SelectFormatCommand = new RelayCommand<ExportOptionItemViewModel>(SelectFormat);
        SelectStepCommand = new RelayCommand<ExportStepItemViewModel>(step => CurrentStep = step?.Number ?? 1);
        NextCommand = new RelayCommand(() => CurrentStep = Math.Min(4, CurrentStep + 1));
        BackCommand = new RelayCommand(() => CurrentStep = Math.Max(1, CurrentStep - 1));
        SelectCitationStyleCommand = new RelayCommand<string>(style => CitationStyle = style ?? "APA");
        ToggleSectionCommand = new RelayCommand<ExportSectionItemViewModel>(ToggleSection);
        ExportCommand = new RelayCommand(ExportDocument);
        OpenLocationCommand = new RelayCommand(OpenLocation, () => !string.IsNullOrWhiteSpace(LastExportPath));
        NewExportCommand = new RelayCommand(StartNewExport);

        SelectFormat(Formats.FirstOrDefault(format => format.IsSelected) ?? Formats.FirstOrDefault());
        UpdateSteps();
    }

    public ObservableCollection<ExportOptionItemViewModel> Formats { get; }
    public ObservableCollection<ExportSectionItemViewModel> Sections { get; }
    public ObservableCollection<ExportStepItemViewModel> Steps { get; }

    public ICommand SelectFormatCommand { get; }
    public ICommand SelectStepCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand SelectCitationStyleCommand { get; }
    public ICommand ToggleSectionCommand { get; }
    public ICommand ExportCommand { get; }
    public RelayCommand OpenLocationCommand { get; }
    public ICommand NewExportCommand { get; }

    public int CurrentStep
    {
        get => currentStep;
        set
        {
            var normalized = Math.Clamp(value, 1, 4);
            if (!SetProperty(ref currentStep, normalized)) return;
            UpdateSteps();
            OnPropertyChanged(nameof(IsFormatStep));
            OnPropertyChanged(nameof(IsOptionsStep));
            OnPropertyChanged(nameof(IsPreviewStep));
            OnPropertyChanged(nameof(IsExportStep));
            OnPropertyChanged(nameof(NextButtonLabel));
        }
    }

    public ExportOptionItemViewModel? SelectedFormat
    {
        get => selectedFormat;
        private set
        {
            if (!SetProperty(ref selectedFormat, value)) return;
            OnPropertyChanged(nameof(FormatLabel));
            OnPropertyChanged(nameof(PreviewFormatLabel));
            OnPropertyChanged(nameof(ExportFileName));
        }
    }

    public string CitationStyle
    {
        get => citationStyle;
        private set
        {
            if (!SetProperty(ref citationStyle, value)) return;
            OnPropertyChanged(nameof(CitationSummary));
            OnPropertyChanged(nameof(IsApa));
            OnPropertyChanged(nameof(IsMla));
            OnPropertyChanged(nameof(IsChicago));
        }
    }

    public bool IncludeCover
    {
        get => includeCover;
        set => SetProperty(ref includeCover, value);
    }

    public bool IncludeTableOfContents
    {
        get => includeTableOfContents;
        set => SetProperty(ref includeTableOfContents, value);
    }

    public bool IncludeBibliography
    {
        get => includeBibliography;
        set => SetProperty(ref includeBibliography, value);
    }

    public bool ExportCompleted
    {
        get => exportCompleted;
        private set => SetProperty(ref exportCompleted, value);
    }

    public string FileName
    {
        get => fileName;
        set
        {
            var safeName = string.IsNullOrWhiteSpace(value) ? "MindLink_Exportacion" : value.Trim();
            if (!SetProperty(ref fileName, safeName)) return;
            OnPropertyChanged(nameof(ExportFileName));
        }
    }

    public string? LastExportPath
    {
        get => lastExportPath;
        private set
        {
            if (!SetProperty(ref lastExportPath, value)) return;
            OnPropertyChanged(nameof(LastExportFileName));
            OpenLocationCommand.RaiseCanExecuteChanged();
        }
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }

    public string ProjectTitle => workspace.ExportSummary.ProjectTitle;
    public string FormatLabel => SelectedFormat?.Title ?? workspace.ExportSummary.Format;
    public string PreviewFormatLabel => $"Vista previa — {FormatLabel}";
    public string CitationSummary => $"{CitationStyle}, 7.ª ed.";
    public string IncludedSectionsLabel => $"{Sections.Count(section => section.IsSelected)} de {Sections.Count}";
    public string PreviewSectionsLabel => $"{Sections.Count(section => section.IsSelected)} secciones · ~{workspace.ExportSummary.EstimatedPages} páginas";
    public string ReferencesLabel => $"{workspace.ExportSummary.IncludedReferences} incluidas";
    public string EstimatedPagesLabel => $"~{workspace.ExportSummary.EstimatedPages} páginas";
    public string ExportFileName => $"{FileName}{SelectedFormat?.Extension ?? ".pdf"}";
    public string LastExportFileName => string.IsNullOrWhiteSpace(LastExportPath) ? ExportFileName : Path.GetFileName(LastExportPath);
    public string NextButtonLabel => CurrentStep switch
    {
        1 => "Siguiente: Opciones →",
        2 => "Vista previa →",
        3 => "Continuar a exportar →",
        _ => "Exportar documento"
    };

    public bool IsFormatStep => CurrentStep == 1;
    public bool IsOptionsStep => CurrentStep == 2;
    public bool IsPreviewStep => CurrentStep == 3;
    public bool IsExportStep => CurrentStep == 4;
    public bool IsApa => CitationStyle == "APA";
    public bool IsMla => CitationStyle == "MLA";
    public bool IsChicago => CitationStyle == "Chicago";
    private WorkspaceSnapshot CurrentWorkspace => session.Snapshot ?? workspace;

    private static IEnumerable<ExportSectionItemViewModel> CreateSections(WorkspaceSnapshot workspace)
    {
        var topLevelSections = workspace.ActiveDocument.Sections
            .Where(section => section.ParentId is null)
            .OrderBy(section => section.Order)
            .Take(3)
            .Select(section => new ExportSectionItemViewModel(
                section.Id, $"Capítulo {section.Number}: {section.Title}", true, false))
            .ToList();

        while (topLevelSections.Count < 3)
        {
            var number = topLevelSections.Count + 1;
            var title = number switch { 1 => "Introducción", 2 => "Marco teórico", _ => "Metodología" };
            topLevelSections.Add(new ExportSectionItemViewModel($"chapter-{number}", $"Capítulo {number}: {title}", true, false));
        }

        topLevelSections.Add(new ExportSectionItemViewModel("appendices", "Anexos", false, true));
        return topLevelSections;
    }

    private void SelectFormat(ExportOptionItemViewModel? format)
    {
        if (format is null) return;
        foreach (var item in Formats) item.IsSelected = ReferenceEquals(item, format);
        SelectedFormat = format;
        StatusMessage = string.Empty;
    }

    private void ToggleSection(ExportSectionItemViewModel? section)
    {
        if (section is null) return;
        section.IsSelected = !section.IsSelected;
        OnPropertyChanged(nameof(IncludedSectionsLabel));
        OnPropertyChanged(nameof(PreviewSectionsLabel));
    }

    private void UpdateSteps()
    {
        foreach (var step in Steps)
        {
            step.IsActive = step.Number == CurrentStep;
            step.IsCompleted = step.Number < CurrentStep;
        }
    }

    private void ExportDocument()
    {
        if (CurrentStep < 4)
        {
            CurrentStep = 4;
            return;
        }

        var format = SelectedFormat ?? Formats[0];
        var dialog = new SaveFileDialog
        {
            Title = "Exportar documento de MindLink",
            FileName = FileName,
            DefaultExt = format.Extension,
            AddExtension = true,
            OverwritePrompt = true,
            Filter = BuildFilter(format.Extension)
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dialog.FileName)!);
            WriteExport(dialog.FileName, format.Extension);
            LastExportPath = dialog.FileName;
            ExportCompleted = true;
            StatusMessage = "Documento exportado correctamente.";
        }
        catch (Exception exception)
        {
            StatusMessage = $"No se pudo exportar el documento: {exception.Message}";
        }
    }

    private void WriteExport(string path, string extension)
    {
        switch (extension.ToLowerInvariant())
        {
            case ".pdf":
                WritePdf(path);
                break;
            case ".docx":
                WriteDocx(path);
                break;
            case ".md":
                File.WriteAllText(path, BuildMarkdown(), new UTF8Encoding(false));
                break;
            default:
                File.WriteAllText(path, BuildPlainText(), new UTF8Encoding(false));
                break;
        }
    }

    private string BuildPlainText()
    {
        var builder = new StringBuilder();
        builder.AppendLine("IMPACTO DE LA INTELIGENCIA ARTIFICIAL EN EL APRENDIZAJE UNIVERSITARIO");
        builder.AppendLine();
        builder.AppendLine(CurrentWorkspace.User.FullName);
        builder.AppendLine("Tesis de maestría en Educación Digital");
        builder.AppendLine();

        foreach (var section in CurrentWorkspace.ActiveDocument.Sections.OrderBy(section => section.Order))
        {
            builder.AppendLine($"{section.Number}. {section.Title}");
            builder.AppendLine(section.Content);
            builder.AppendLine();
        }

        if (IncludeBibliography)
        {
            builder.AppendLine("BIBLIOGRAFÍA");
            foreach (var reference in CurrentWorkspace.References)
            {
                builder.AppendLine($"{reference.Authors} ({reference.Year}). {reference.Title}. {reference.Source}.");
            }
        }

        return builder.ToString();
    }

    private string BuildMarkdown()
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Impacto de la inteligencia artificial en el aprendizaje universitario");
        builder.AppendLine();
        builder.AppendLine($"**Autoría:** {CurrentWorkspace.User.FullName}  ");
        builder.AppendLine("**Programa:** Maestría en Educación Digital");
        builder.AppendLine();

        foreach (var section in CurrentWorkspace.ActiveDocument.Sections.OrderBy(section => section.Order))
        {
            builder.AppendLine($"## {section.Number}. {section.Title}");
            builder.AppendLine();
            builder.AppendLine(section.Content);
            builder.AppendLine();
        }

        if (IncludeBibliography)
        {
            builder.AppendLine("## Bibliografía");
            builder.AppendLine();
            foreach (var reference in CurrentWorkspace.References)
            {
                builder.AppendLine($"- {reference.Authors} ({reference.Year}). *{reference.Title}*. {reference.Source}.");
            }
        }

        return builder.ToString();
    }

    private void WriteDocx(string path)
    {
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        WriteZipEntry(archive, "[Content_Types].xml",
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/></Types>");
        WriteZipEntry(archive, "_rels/.rels",
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"word/document.xml\"/></Relationships>");

        var paragraphs = BuildPlainText().Split(Environment.NewLine)
            .Select(line => $"<w:p><w:r><w:t xml:space=\"preserve\">{SecurityElement.Escape(line) ?? string.Empty}</w:t></w:r></w:p>");
        var document = $"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body>{string.Concat(paragraphs)}<w:sectPr><w:pgSz w:w=\"12240\" w:h=\"15840\"/></w:sectPr></w:body></w:document>";
        WriteZipEntry(archive, "word/document.xml", document);
    }

    private static void WriteZipEntry(ZipArchive archive, string name, string contents)
    {
        var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(contents);
    }

    private void WritePdf(string path)
    {
        static string EscapePdf(string value) => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        static string ToAscii(string value)
        {
            var normalized = value.Normalize(NormalizationForm.FormD);
            return new string(normalized.Where(character => character <= 127 &&
                System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray());
        }

        var lines = BuildPlainText()
            .Split(Environment.NewLine)
            .SelectMany(line => SplitPdfLine(ToAscii(line), 82))
            .Take(48)
            .ToArray();
        var contentBuilder = new StringBuilder("BT /F1 16 Tf 72 744 Td ");
        for (var index = 0; index < lines.Length; index++)
        {
            if (index == 1) contentBuilder.Append("/F1 11 Tf ");
            contentBuilder.Append($"({EscapePdf(lines[index])}) Tj 0 -15 Td ");
        }
        contentBuilder.Append("ET");
        var content = contentBuilder.ToString();
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}\nendstream"
        };

        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(stream, Encoding.ASCII, 1024, true) { NewLine = "\n" };
        writer.Write("%PDF-1.4\n");
        writer.Flush();
        var offsets = new List<long> { 0 };
        for (var index = 0; index < objects.Length; index++)
        {
            offsets.Add(stream.Position);
            writer.Write($"{index + 1} 0 obj\n{objects[index]}\nendobj\n");
            writer.Flush();
        }

        var xref = stream.Position;
        writer.Write($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1)) writer.Write($"{offset:0000000000} 00000 n \n");
        writer.Write($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
    }

    private static IEnumerable<string> SplitPdfLine(string line, int maximumLength)
    {
        if (string.IsNullOrEmpty(line)) return [string.Empty];
        var parts = new List<string>();
        while (line.Length > maximumLength)
        {
            var split = line.LastIndexOf(' ', maximumLength);
            if (split <= 0) split = maximumLength;
            parts.Add(line[..split]);
            line = line[split..].TrimStart();
        }
        parts.Add(line);
        return parts;
    }

    private static string BuildFilter(string extension) => extension.ToLowerInvariant() switch
    {
        ".pdf" => "Documento PDF (*.pdf)|*.pdf",
        ".docx" => "Documento de Word (*.docx)|*.docx",
        ".md" => "Markdown (*.md)|*.md",
        _ => "Texto plano (*.txt)|*.txt"
    };

    private void OpenLocation()
    {
        if (string.IsNullOrWhiteSpace(LastExportPath)) return;
        Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{LastExportPath}\"") { UseShellExecute = true });
    }

    private void StartNewExport()
    {
        ExportCompleted = false;
        StatusMessage = string.Empty;
        CurrentStep = 1;
    }
}

public sealed class ExportOptionItemViewModel : ObservableObject
{
    private bool isSelected;

    public ExportOptionItemViewModel(ExportOption option)
    {
        Id = option.Id;
        Title = option.Title;
        DisplayTitle = option.Title == "Word" ? "Word (.docx)" : option.Title;
        Extension = option.FileExtension;
        Description = option.Description.TrimEnd('.');
        IconKey = option.IconKey;
        IsRecommended = option.IsRecommended;
        isSelected = option.IsSelected;
        Icon = option.IconKey switch
        {
            "Pdf" => "📄",
            "Word" => "📝",
            "Markdown" => "📋",
            _ => "🗒"
        };
    }

    public string Id { get; }
    public string Title { get; }
    public string DisplayTitle { get; }
    public string Extension { get; }
    public string Description { get; }
    public string IconKey { get; }
    public string Icon { get; }
    public bool IsRecommended { get; }

    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }
}

public sealed class ExportSectionItemViewModel(string id, string title, bool isSelected, bool isOptional) : ObservableObject
{
    private bool selected = isSelected;
    public string Id { get; } = id;
    public string Title { get; } = title;
    public bool IsOptional { get; } = isOptional;
    public bool IsSelected { get => selected; set => SetProperty(ref selected, value); }
}

public sealed class ExportStepItemViewModel(int number, string label) : ObservableObject
{
    private bool isActive;
    private bool isCompleted;
    public int Number { get; } = number;
    public string Label { get; } = label;
    public bool IsActive { get => isActive; set => SetProperty(ref isActive, value); }
    public bool IsCompleted { get => isCompleted; set => SetProperty(ref isCompleted, value); }
    public string Marker => IsCompleted ? "✓" : Number.ToString();
}
