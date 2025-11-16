using Windows.System;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Marechai.App.Presentation.Views;

public sealed partial class LoginPage : Page
{
    public LoginPage() => InitializeComponent();

    private void OnEmailKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if(e.Key == VirtualKey.Enter)
        {
            // Move focus to password field
            PasswordBox.Focus(FocusState.Keyboard);
            e.Handled = true;
        }
    }

    private void OnPasswordKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if(e.Key == VirtualKey.Enter)
        {
            // Trigger login when Enter is pressed
            if(DataContext is LoginViewModel viewModel && LoginButton.IsEnabled) viewModel.LoginCommand.Execute(null);

            e.Handled = true;
        }
    }
}