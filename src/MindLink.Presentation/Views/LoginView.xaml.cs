using System.Windows;
using System.Windows.Controls;
using MindLink.Presentation.ViewModels;

namespace MindLink.Presentation.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
        DataContextChanged += SetInitialPassword;
    }

    private void SetInitialPassword(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel && PasswordInput.Password.Length == 0)
            PasswordInput.Password = viewModel.Password;
    }

    private void PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel)
            viewModel.Password = ((PasswordBox)sender).Password;
    }
}
