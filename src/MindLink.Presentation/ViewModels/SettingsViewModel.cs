using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MindLink.Application.Models;

namespace MindLink.Presentation.ViewModels;

public sealed class SettingsViewModel : ObservableObject
{
    private SettingsSectionItemViewModel? selectedSection;
    private string selectedTheme = "Claro";
    private int editorFontSize = 15;
    private bool encryptLocalFiles = true;
    private bool allowUsageDiagnostics;
    private bool confirmBeforeDeleting = true;
    private bool cloudSyncRequested;
    private string defaultCitationStyle = "APA";
    private string statusMessage = string.Empty;

    public SettingsViewModel(WorkspaceSnapshot workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        UserName = workspace.User.FullName;
        UserRole = workspace.User.Role;
        IsPaidPlan = workspace.User.Role == "Plan Pro";
        cloudSyncRequested = IsPaidPlan;

        Sections = new ObservableCollection<SettingsSectionItemViewModel>
        {
            new("appearance", "Apariencia"),
            new("storage", "Almacenamiento"),
            new("privacy", "Privacidad"),
            new("citations", "Citas y formato"),
            new("shortcuts", "Atajos de teclado"),
            new("about", "Acerca de MindLink")
        };
        FontSizes = new ObservableCollection<EditorFontSizeItemViewModel>
        {
            new(13, "Pequeño (13px)"),
            new(15, "Mediano (15px)"),
            new(17, "Grande (17px)")
        };
        Shortcuts = new ObservableCollection<KeyboardShortcutItemViewModel>
        {
            new("Nueva nota", "Ctrl + N"),
            new("Búsqueda global", "Ctrl + K"),
            new("Guardar documento", "Ctrl + S"),
            new("Insertar cita", "Ctrl + Alt + C"),
            new("Abrir red de conocimiento", "Ctrl + G"),
            new("Exportar documento", "Ctrl + E"),
            new("Modo enfoque", "Ctrl + Shift + F"),
            new("Nueva referencia", "Ctrl + Alt + R")
        };

        SelectSectionCommand = new RelayCommand<SettingsSectionItemViewModel>(SelectSection);
        SelectThemeCommand = new RelayCommand<string>(theme => ApplyTheme(theme ?? "Claro"));
        SelectFontSizeCommand = new RelayCommand<EditorFontSizeItemViewModel>(SelectFontSize);
        SelectCitationStyleCommand = new RelayCommand<string>(style => DefaultCitationStyle = style ?? "APA");
        OpenStorageFolderCommand = new RelayCommand(OpenStorageFolder);
        ActivateCloudSyncCommand = new RelayCommand(ActivateCloudSync, () => IsPaidPlan);

        SelectSection(Sections[0]);
        SelectFontSize(FontSizes.First(item => item.Size == 15));
    }

    public ObservableCollection<SettingsSectionItemViewModel> Sections { get; }
    public ObservableCollection<EditorFontSizeItemViewModel> FontSizes { get; }
    public ObservableCollection<KeyboardShortcutItemViewModel> Shortcuts { get; }

    public ICommand SelectSectionCommand { get; }
    public ICommand SelectThemeCommand { get; }
    public ICommand SelectFontSizeCommand { get; }
    public ICommand SelectCitationStyleCommand { get; }
    public ICommand OpenStorageFolderCommand { get; }
    public ICommand ActivateCloudSyncCommand { get; }

    public SettingsSectionItemViewModel? SelectedSection
    {
        get => selectedSection;
        private set
        {
            if (!SetProperty(ref selectedSection, value)) return;
            OnPropertyChanged(nameof(IsAppearanceSection));
            OnPropertyChanged(nameof(IsStorageSection));
            OnPropertyChanged(nameof(IsPrivacySection));
            OnPropertyChanged(nameof(IsCitationsSection));
            OnPropertyChanged(nameof(IsShortcutsSection));
            OnPropertyChanged(nameof(IsAboutSection));
        }
    }

    public string SelectedTheme
    {
        get => selectedTheme;
        private set
        {
            if (!SetProperty(ref selectedTheme, value)) return;
            OnPropertyChanged(nameof(IsLightTheme));
            OnPropertyChanged(nameof(IsDarkTheme));
        }
    }

    public int EditorFontSize
    {
        get => editorFontSize;
        private set => SetProperty(ref editorFontSize, value);
    }

    public bool EncryptLocalFiles
    {
        get => encryptLocalFiles;
        set
        {
            if (!SetProperty(ref encryptLocalFiles, value)) return;
            StatusMessage = value ? "Cifrado local activado." : "Cifrado local desactivado.";
        }
    }

    public bool AllowUsageDiagnostics
    {
        get => allowUsageDiagnostics;
        set
        {
            if (!SetProperty(ref allowUsageDiagnostics, value)) return;
            StatusMessage = value ? "Diagnósticos anónimos habilitados." : "Diagnósticos anónimos deshabilitados.";
        }
    }

    public bool ConfirmBeforeDeleting
    {
        get => confirmBeforeDeleting;
        set => SetProperty(ref confirmBeforeDeleting, value);
    }

    public bool CloudSyncRequested
    {
        get => cloudSyncRequested;
        private set => SetProperty(ref cloudSyncRequested, value);
    }

    public string DefaultCitationStyle
    {
        get => defaultCitationStyle;
        private set
        {
            if (!SetProperty(ref defaultCitationStyle, value)) return;
            OnPropertyChanged(nameof(IsApa));
            OnPropertyChanged(nameof(IsMla));
            OnPropertyChanged(nameof(IsChicago));
            StatusMessage = $"Formato predeterminado cambiado a {value}.";
        }
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }

    public string UserName { get; }
    public string UserRole { get; }
    public bool IsPaidPlan { get; }
    public string StorageLocation => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MindLink");
    public string StorageUsage => "847 MB / Sin límite";
    public bool IsAppearanceSection => SelectedSection?.Id == "appearance";
    public bool IsStorageSection => SelectedSection?.Id == "storage";
    public bool IsPrivacySection => SelectedSection?.Id == "privacy";
    public bool IsCitationsSection => SelectedSection?.Id == "citations";
    public bool IsShortcutsSection => SelectedSection?.Id == "shortcuts";
    public bool IsAboutSection => SelectedSection?.Id == "about";
    public bool IsLightTheme => SelectedTheme == "Claro";
    public bool IsDarkTheme => SelectedTheme == "Oscuro";
    public bool IsApa => DefaultCitationStyle == "APA";
    public bool IsMla => DefaultCitationStyle == "MLA";
    public bool IsChicago => DefaultCitationStyle == "Chicago";

    private void SelectSection(SettingsSectionItemViewModel? section)
    {
        if (section is null) return;
        foreach (var item in Sections) item.IsSelected = ReferenceEquals(item, section);
        SelectedSection = section;
        StatusMessage = string.Empty;
    }

    private void ApplyTheme(string theme)
    {
        SelectedTheme = theme == "Oscuro" ? "Oscuro" : "Claro";
        var palette = IsDarkTheme
            ? new Dictionary<string, string>
            {
                ["AppBackground"] = "#0E1B2E",
                ["Surface"] = "#16253A",
                ["TextPrimary"] = "#F4F8FA",
                ["TextSecondary"] = "#C5D8E2",
                ["TextMuted"] = "#9AB4C2",
                ["TextSubtle"] = "#718B9A",
                ["Border"] = "#2B425A",
                ["SoftTeal"] = "#123844",
                ["SoftBlue"] = "#172E4B",
                ["SoftGreen"] = "#173B31",
                ["SoftAmber"] = "#40361F",
                ["SoftRed"] = "#442728"
            }
            : new Dictionary<string, string>
            {
                ["AppBackground"] = "#F4F8FA",
                ["Surface"] = "#FFFFFF",
                ["TextPrimary"] = "#102A43",
                ["TextSecondary"] = "#4A6B7E",
                ["TextMuted"] = "#6B8FA3",
                ["TextSubtle"] = "#9AB4C2",
                ["Border"] = "#D9E7EC",
                ["SoftTeal"] = "#ECF8F8",
                ["SoftBlue"] = "#EDF4F8",
                ["SoftGreen"] = "#EDF9F2",
                ["SoftAmber"] = "#FFF8E9",
                ["SoftRed"] = "#FFF1F0"
            };

        if (System.Windows.Application.Current is not null)
        {
            foreach (var (key, value) in palette)
            {
                System.Windows.Application.Current.Resources[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
            }
        }

        StatusMessage = $"Tema {SelectedTheme.ToLowerInvariant()} aplicado.";
    }

    private void SelectFontSize(EditorFontSizeItemViewModel? fontSize)
    {
        if (fontSize is null) return;
        foreach (var item in FontSizes) item.IsSelected = ReferenceEquals(item, fontSize);
        EditorFontSize = fontSize.Size;
        if (System.Windows.Application.Current is not null) System.Windows.Application.Current.Resources["EditorFontSize"] = (double)fontSize.Size;
        StatusMessage = $"Tamaño del editor establecido en {fontSize.Size}px.";
    }

    private void OpenStorageFolder()
    {
        Directory.CreateDirectory(StorageLocation);
        Process.Start(new ProcessStartInfo(StorageLocation) { UseShellExecute = true });
        StatusMessage = "Carpeta local abierta.";
    }

    private void ActivateCloudSync()
    {
        if (!IsPaidPlan)
        {
            StatusMessage = "La sincronizacion cifrada esta disponible con el plan Pro.";
            return;
        }
        CloudSyncRequested = true;
        StatusMessage = "Solicitud registrada. La sincronización es un servicio opcional de MindLink.";
    }
}

public sealed class SettingsSectionItemViewModel(string id, string label) : ObservableObject
{
    private bool isSelected;
    public string Id { get; } = id;
    public string Label { get; } = label;
    public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
}

public sealed class EditorFontSizeItemViewModel(int size, string label) : ObservableObject
{
    private bool isSelected;
    public int Size { get; } = size;
    public string Label { get; } = label;
    public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
}

public sealed class KeyboardShortcutItemViewModel(string action, string shortcut)
{
    public string Action { get; } = action;
    public string Shortcut { get; } = shortcut;
}
