using System;
using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminWwpcImportsPage : Page
{
    public AdminWwpcImportsPage() => InitializeComponent();

    AdminWwpcImportsViewModel? ViewModel => DataContext as AdminWwpcImportsViewModel;

    private async void Reject_Click(object sender, RoutedEventArgs e)
    {
        if((sender as Button)?.CommandParameter is not WwpcQueueItemViewModel item ||
           DataContext is not AdminWwpcImportsViewModel vm) return;

        var dialog = new ContentDialog
        {
            XamlRoot            = XamlRoot,
            Title               = "Reject WinWorldPC import",
            Content             = $"Are you sure you want to reject '{item.Name}'?",
            PrimaryButtonText   = "Reject",
            CloseButtonText     = "Cancel",
            DefaultButton       = ContentDialogButton.Close
        };

        ContentDialogResult result = await dialog.ShowAsync();
        if(result == ContentDialogResult.Primary)
            await vm.RejectAsync(item);
    }

    private async void Duplicate_Click(object sender, RoutedEventArgs e)
    {
        if((sender as Button)?.CommandParameter is not WwpcQueueItemViewModel item ||
           DataContext is not AdminWwpcImportsViewModel vm) return;

        var nameBox = new TextBox
        {
            Text = $"{item.Name} (Dup)"
        };

        var dialog = new ContentDialog
        {
            XamlRoot          = XamlRoot,
            Title             = "Duplicate WinWorldPC import",
            Content           = nameBox,
            PrimaryButtonText = "Duplicate",
            CloseButtonText   = "Cancel",
            DefaultButton     = ContentDialogButton.Primary
        };

        ContentDialogResult result = await dialog.ShowAsync();
        if(result == ContentDialogResult.Primary)
            await vm.DuplicateAsync(item, nameBox.Text);
    }

    private async void OpenSource_Click(object sender, RoutedEventArgs e)
    {
        if((sender as HyperlinkButton)?.CommandParameter is not WwpcQueueItemViewModel item ||
           DataContext is not AdminWwpcImportsViewModel vm) return;

        string url = vm.AbsoluteWwpcUrl(item.SourceUrl);
        if(Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            await Launcher.LaunchUriAsync(uri);
    }

    private async void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(ViewModel == null) return;

        ViewModel.CurrentPage = 1;
        await ViewModel.LoadCommand.ExecuteAsync(null);
    }
}
