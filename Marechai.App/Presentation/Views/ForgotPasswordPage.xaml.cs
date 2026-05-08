using Windows.System;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Marechai.App.Presentation.Views;

public sealed partial class ForgotPasswordPage : Page
{
    public ForgotPasswordPage() => InitializeComponent();

    private void OnEmailKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if(e.Key != VirtualKey.Enter) return;

        if(DataContext is ForgotPasswordViewModel vm && !vm.IsSubmitting) vm.SubmitCommand.Execute(null);

        e.Handled = true;
    }
}
