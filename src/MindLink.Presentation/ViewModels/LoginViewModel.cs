using MindLink.Application.Interfaces;
using MindLink.Presentation.Navigation;

namespace MindLink.Presentation.ViewModels;

public sealed class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService authentication;
    private readonly NavigationService navigation;
    private string email = "jonny@mindlink.local";
    private string password = "MindLink2026!";
    private string errorMessage = string.Empty;
    private bool isBusy;

    public string Email { get => email; set { if (SetProperty(ref email, value)) SignInCommand.RaiseCanExecuteChanged(); } }
    public string Password { get => password; set { if (SetProperty(ref password, value)) SignInCommand.RaiseCanExecuteChanged(); } }
    public string ErrorMessage { get => errorMessage; private set => SetProperty(ref errorMessage, value); }
    public bool IsBusy { get => isBusy; private set { if (SetProperty(ref isBusy, value)) SignInCommand.RaiseCanExecuteChanged(); } }
    public RelayCommand SignInCommand { get; }

    public LoginViewModel(IAuthenticationService authentication, NavigationService navigation)
    {
        this.authentication = authentication;
        this.navigation = navigation;
        SignInCommand = new RelayCommand(() => _ = SignInAsync(), () => !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password));
    }

    private async Task SignInAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;
        try
        {
            var user = await authentication.SignInAsync(Email, Password);
            if (user is null)
            {
                ErrorMessage = "No pudimos validar tus credenciales. Revisa el correo y la contraseña.";
                return;
            }

            AppSession.UserId = user.Id;
            AppSession.UserName = user.FullName;
            navigation.NavigateToDashboard();
        }
        catch { ErrorMessage = "Ocurrió un problema al abrir tu espacio local. Inténtalo nuevamente."; }
        finally { IsBusy = false; }
    }
}

public static class AppSession
{
    public static int UserId { get; set; }
    public static string UserName { get; set; } = string.Empty;
}
