using MindLink.Presentation.Navigation;

namespace MindLink.Presentation.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private ObservableObject? currentViewModel;

    public MainViewModel(NavigationService navigation)
    {
        navigation.RootNavigated += viewModel => CurrentViewModel = viewModel;
        navigation.ShowWelcome();
    }

    public ObservableObject? CurrentViewModel
    {
        get => currentViewModel;
        private set => SetProperty(ref currentViewModel, value);
    }
}
