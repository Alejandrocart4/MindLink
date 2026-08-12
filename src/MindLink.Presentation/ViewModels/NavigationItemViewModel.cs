using MindLink.Presentation.Navigation;

namespace MindLink.Presentation.ViewModels;

public sealed class NavigationItemViewModel : ObservableObject
{
    private bool isSelected;

    public NavigationItemViewModel(
        string label,
        string glyph,
        WorkspaceRoute route,
        Action<WorkspaceRoute> navigate)
    {
        Label = label;
        Glyph = glyph;
        Route = route;
        NavigateCommand = new RelayCommand(() => navigate(Route));
    }

    public string Label { get; }
    public string Glyph { get; }
    public WorkspaceRoute Route { get; }
    public RelayCommand NavigateCommand { get; }

    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }
}
