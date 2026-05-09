using Windows.System;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Marechai.App.Presentation.Views;

public sealed partial class ResendConfirmationPage : Page
{
    public ResendConfirmationPage() => InitializeComponent();

    private void OnEmailKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if(e.Key != VirtualKey.Enter) return;

        if(DataContext is ResendConfirmationViewModel vm && !vm.IsSubmitting) vm.SubmitCommand.Execute(null);

        e.Handled = true;
    }
}
