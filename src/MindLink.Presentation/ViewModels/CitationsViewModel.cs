using System.Collections.ObjectModel;
using System.Windows;
using MindLink.Application.Models;

namespace MindLink.Presentation.ViewModels;

public sealed class CitationsViewModel : ObservableObject
{
    private string feedbackMessage = string.Empty;
    private bool isFeedbackVisible;
    private bool isCreatingCitation;
    private string newQuote = string.Empty;
    private string newSource = string.Empty;
    private string newPage = string.Empty;
    private string newContext = string.Empty;

    public CitationsViewModel(WorkspaceSnapshot workspace)
    {
        Citations = new ObservableCollection<CitationCardViewModel>(
            workspace.Citations.Select(citation =>
            {
                var reference = workspace.References.FirstOrDefault(item => item.Id == citation.ReferenceId);
                var note = workspace.Notes.FirstOrDefault(item => item.Id == citation.NoteId);
                return new CitationCardViewModel(citation, reference, note);
            }));

        ToggleNewCitationCommand = new RelayCommand(ToggleNewCitation);
        SaveNewCitationCommand = new RelayCommand(SaveNewCitation, CanSaveNewCitation);
        CancelNewCitationCommand = new RelayCommand(CancelNewCitation);
        CopyApaCommand = new RelayCommand<CitationCardViewModel>(CopyApa);
        InsertIntoDocumentCommand = new RelayCommand<CitationCardViewModel>(InsertIntoDocument);
        DismissFeedbackCommand = new RelayCommand(() => IsFeedbackVisible = false);
    }

    public ObservableCollection<CitationCardViewModel> Citations { get; }
    public string CitationSummary => $"{Citations.Count} citas guardadas · Formato activo: APA 7ª ed.";

    public string FeedbackMessage
    {
        get => feedbackMessage;
        private set => SetProperty(ref feedbackMessage, value);
    }

    public bool IsFeedbackVisible
    {
        get => isFeedbackVisible;
        private set => SetProperty(ref isFeedbackVisible, value);
    }

    public bool IsCreatingCitation
    {
        get => isCreatingCitation;
        private set => SetProperty(ref isCreatingCitation, value);
    }

    public string NewQuote
    {
        get => newQuote;
        set
        {
            if (SetProperty(ref newQuote, value)) SaveNewCitationCommand.RaiseCanExecuteChanged();
        }
    }

    public string NewSource
    {
        get => newSource;
        set
        {
            if (SetProperty(ref newSource, value)) SaveNewCitationCommand.RaiseCanExecuteChanged();
        }
    }

    public string NewPage
    {
        get => newPage;
        set => SetProperty(ref newPage, value);
    }

    public string NewContext
    {
        get => newContext;
        set => SetProperty(ref newContext, value);
    }

    public RelayCommand ToggleNewCitationCommand { get; }
    public RelayCommand SaveNewCitationCommand { get; }
    public RelayCommand CancelNewCitationCommand { get; }
    public RelayCommand<CitationCardViewModel> CopyApaCommand { get; }
    public RelayCommand<CitationCardViewModel> InsertIntoDocumentCommand { get; }
    public RelayCommand DismissFeedbackCommand { get; }

    private void ToggleNewCitation()
    {
        IsCreatingCitation = !IsCreatingCitation;
        IsFeedbackVisible = false;
    }

    private bool CanSaveNewCitation() =>
        !string.IsNullOrWhiteSpace(NewQuote) && !string.IsNullOrWhiteSpace(NewSource);

    private void SaveNewCitation()
    {
        var card = CitationCardViewModel.CreateLocalDraft(
            NewQuote.Trim(),
            NewSource.Trim(),
            NewPage.Trim(),
            NewContext.Trim());
        Citations.Insert(0, card);
        OnPropertyChanged(nameof(CitationSummary));
        CancelNewCitation();
        ShowFeedback("La nueva cita quedó guardada en este espacio local.");
    }

    private void CancelNewCitation()
    {
        IsCreatingCitation = false;
        NewQuote = string.Empty;
        NewSource = string.Empty;
        NewPage = string.Empty;
        NewContext = string.Empty;
    }

    private void CopyApa(CitationCardViewModel? citation)
    {
        if (citation is null) return;
        try
        {
            Clipboard.SetText(citation.ApaText);
            ShowFeedback($"Cita de {citation.Source} copiada en formato APA.");
        }
        catch
        {
            ShowFeedback("No fue posible acceder al portapapeles. Intenta copiarla nuevamente.");
        }
    }

    private void InsertIntoDocument(CitationCardViewModel? citation)
    {
        if (citation is null) return;
        ShowFeedback($"La cita de {citation.Source} quedó preparada para insertarse en Documentos.");
    }

    private void ShowFeedback(string message)
    {
        FeedbackMessage = message;
        IsFeedbackVisible = true;
    }
}

public sealed class CitationCardViewModel
{
    public CitationCardViewModel(WorkspaceCitation citation, WorkspaceReference? reference, WorkspaceNote? note)
    {
        Id = citation.Id;
        Quote = citation.Quote.Trim().Trim('“', '”', '"');
        var sourceParts = citation.SourceLabel.Split('·', 2, StringSplitOptions.TrimEntries);
        Source = sourceParts[0];
        Work = sourceParts.Length > 1 ? sourceParts[1] : reference?.Title ?? string.Empty;
        Page = citation.PageLabel;
        Context = citation.Context;
        LinkedNote = note?.Title ?? "Nota de investigación";
        Tags = citation.Tags;
        ApaText = $"{Source}. {Work}. {Page}. “{Quote}”";
    }

    private CitationCardViewModel(string quote, string source, string page, string context)
    {
        Id = $"local-{Guid.NewGuid():N}";
        Quote = quote.Trim().Trim('“', '”', '"');
        Source = source;
        Work = "Cita incorporada manualmente";
        Page = string.IsNullOrWhiteSpace(page) ? "s. p." : page.StartsWith("p.", StringComparison.OrdinalIgnoreCase) ? page : $"p. {page}";
        Context = string.IsNullOrWhiteSpace(context) ? "Pendiente de clasificar" : context;
        LinkedNote = "Sin nota vinculada";
        Tags = ["Nueva"];
        ApaText = $"{Source}. {Work}. {Page}. “{Quote}”";
    }

    public string Id { get; }
    public string Quote { get; }
    public string Source { get; }
    public string Work { get; }
    public string Page { get; }
    public string Context { get; }
    public string LinkedNote { get; }
    public IReadOnlyList<string> Tags { get; }
    public string ApaText { get; }

    public static CitationCardViewModel CreateLocalDraft(string quote, string source, string page, string context) =>
        new(quote, source, page, context);
}
