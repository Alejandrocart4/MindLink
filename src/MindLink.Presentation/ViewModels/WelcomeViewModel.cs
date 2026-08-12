using MindLink.Presentation.Navigation;
using MindLink.Presentation.Services;

namespace MindLink.Presentation.ViewModels;

public sealed class WelcomeViewModel : ObservableObject
{
    public IReadOnlyList<string> Features { get; } =
    [
        "Notas y referencias integradas",
        "Red de conocimiento interactiva",
        "Redacción y citas automáticas",
        "Exportación a PDF, Word, Markdown"
    ];

    public AsyncRelayCommand StartCommand { get; }
    public AsyncRelayCommand OpenProjectCommand { get; }

    public WelcomeViewModel(NavigationService navigation, IFileDialogService fileDialogService)
    {
        StartCommand = new AsyncRelayCommand(() => navigation.OpenWorkspaceAsync());
        OpenProjectCommand = new AsyncRelayCommand(async () =>
        {
            var selectedFile = fileDialogService.SelectProjectFile();
            if (selectedFile is null) return;
            await navigation.OpenWorkspaceAsync(WorkspaceRoute.Documents);
        });
    }
}
