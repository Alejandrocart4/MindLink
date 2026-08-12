using System.Collections.ObjectModel;
using MindLink.Application.Models;

namespace MindLink.Presentation.ViewModels;

public sealed class HistoryViewModel : ObservableObject
{
    private HistoryVersionItemViewModel selectedVersion;
    private bool isRestoreConfirmationVisible;
    private string restorationMessage = string.Empty;
    private bool isRestorationMessageVisible;

    public HistoryViewModel(WorkspaceSnapshot workspace)
    {
        Versions = new ObservableCollection<HistoryVersionItemViewModel>(
            workspace.Versions.Select(version => new HistoryVersionItemViewModel(version)));
        selectedVersion = Versions.FirstOrDefault(version => version.IsCurrent) ?? Versions.First();
        selectedVersion.IsSelected = true;

        SelectVersionCommand = new RelayCommand<HistoryVersionItemViewModel>(SelectVersion);
        RequestRestoreCommand = new RelayCommand(RequestRestore);
        ConfirmRestoreCommand = new RelayCommand(ConfirmRestore);
        CancelRestoreCommand = new RelayCommand(() => IsRestoreConfirmationVisible = false);
        DismissMessageCommand = new RelayCommand(() => IsRestorationMessageVisible = false);
    }

    public ObservableCollection<HistoryVersionItemViewModel> Versions { get; }
    public string VersionSummary => $"Marco Teórico · {Versions.Count} versiones";

    public HistoryVersionItemViewModel SelectedVersion
    {
        get => selectedVersion;
        private set
        {
            if (!SetProperty(ref selectedVersion, value)) return;
            OnPropertyChanged(nameof(PreviousVersion));
            OnPropertyChanged(nameof(ComparisonLabel));
            OnPropertyChanged(nameof(PreviousText));
            OnPropertyChanged(nameof(CurrentText));
            OnPropertyChanged(nameof(AddedLabel));
            OnPropertyChanged(nameof(RemovedLabel));
        }
    }

    public HistoryVersionItemViewModel PreviousVersion
    {
        get
        {
            var index = Versions.IndexOf(SelectedVersion);
            return index >= 0 && index < Versions.Count - 1 ? Versions[index + 1] : SelectedVersion;
        }
    }

    public string ComparisonLabel =>
        $"{PreviousVersion.Label} → {SelectedVersion.Label}{(SelectedVersion.IsCurrent ? " actual" : string.Empty)} · {SelectedVersion.SectionLabel} · {SelectedVersion.TimeLabel}";

    public string PreviousText =>
        SelectedVersion.Changes.FirstOrDefault()?.PreviousText ?? PreviousVersion.Summary;

    public string CurrentText =>
        SelectedVersion.Changes.FirstOrDefault()?.CurrentText ?? SelectedVersion.Summary;

    public string AddedLabel => $"+{SelectedVersion.AddedWords:N0}";
    public string RemovedLabel => $"-{SelectedVersion.RemovedWords:N0}";

    public bool IsRestoreConfirmationVisible
    {
        get => isRestoreConfirmationVisible;
        private set => SetProperty(ref isRestoreConfirmationVisible, value);
    }

    public string RestorationMessage
    {
        get => restorationMessage;
        private set => SetProperty(ref restorationMessage, value);
    }

    public bool IsRestorationMessageVisible
    {
        get => isRestorationMessageVisible;
        private set => SetProperty(ref isRestorationMessageVisible, value);
    }

    public RelayCommand<HistoryVersionItemViewModel> SelectVersionCommand { get; }
    public RelayCommand RequestRestoreCommand { get; }
    public RelayCommand ConfirmRestoreCommand { get; }
    public RelayCommand CancelRestoreCommand { get; }
    public RelayCommand DismissMessageCommand { get; }

    private void SelectVersion(HistoryVersionItemViewModel? version)
    {
        if (version is null) return;
        SelectedVersion.IsSelected = false;
        version.IsSelected = true;
        SelectedVersion = version;
        IsRestoreConfirmationVisible = false;
        IsRestorationMessageVisible = false;
    }

    private void RequestRestore()
    {
        if (SelectedVersion.IsCurrent)
        {
            RestorationMessage = "Esta versión ya es la versión actual del documento.";
            IsRestorationMessageVisible = true;
            return;
        }

        IsRestorationMessageVisible = false;
        IsRestoreConfirmationVisible = true;
    }

    private void ConfirmRestore()
    {
        foreach (var version in Versions) version.IsCurrent = version == SelectedVersion;
        IsRestoreConfirmationVisible = false;
        RestorationMessage = $"{SelectedVersion.Label} fue restaurada y ahora es la versión actual.";
        IsRestorationMessageVisible = true;
        OnPropertyChanged(nameof(ComparisonLabel));
    }
}

public sealed class HistoryVersionItemViewModel : ObservableObject
{
    private bool isSelected;
    private bool isCurrent;

    public HistoryVersionItemViewModel(DocumentVersion version)
    {
        Id = version.Id;
        Label = version.Label;
        isCurrent = version.IsCurrent;
        TimeLabel = version.TimeLabel;
        SectionLabel = version.SectionLabel;
        AddedWords = version.AddedWords;
        RemovedWords = version.RemovedWords;
        Summary = version.Summary;
        Changes = version.Changes;
    }

    public string Id { get; }
    public string Label { get; }
    public string TimeLabel { get; }
    public string SectionLabel { get; }
    public int AddedWords { get; }
    public int RemovedWords { get; }
    public string Summary { get; }
    public IReadOnlyList<VersionChange> Changes { get; }
    public string DisplayLabel => IsCurrent ? $"{Label} — actual" : Label;
    public string AddedLabel => $"+{AddedWords:N0}";
    public string RemovedLabel => RemovedWords == 0 ? string.Empty : $"-{RemovedWords:N0}";

    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }

    public bool IsCurrent
    {
        get => isCurrent;
        set
        {
            if (!SetProperty(ref isCurrent, value)) return;
            OnPropertyChanged(nameof(DisplayLabel));
        }
    }
}
