using System;
using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminOldDosImportsPage : Page
{
    public AdminOldDosImportsPage() => InitializeComponent();

    AdminOldDosImportsViewModel? ViewModel => DataContext as AdminOldDosImportsViewModel;

    private async void Reject_Click(object sender, RoutedEventArgs e)
    {
        if((sender as Button)?.CommandParameter is not OldDosQueueItemViewModel item ||
           DataContext is not AdminOldDosImportsViewModel vm) return;

        var dialog = new ContentDialog
        {
            XamlRoot          = XamlRoot,
            Title             = "Reject old-dos import",
            Content           = $"Are you sure you want to reject '{item.Name}'?",
            PrimaryButtonText = "Reject",
            CloseButtonText   = "Cancel",
            DefaultButton     = ContentDialogButton.Close
        };

        ContentDialogResult result = await dialog.ShowAsync();
        if(result == ContentDialogResult.Primary)
            await vm.RejectAsync(item);
    }

    private async void OpenSource_Click(object sender, RoutedEventArgs e)
    {
        if((sender as HyperlinkButton)?.CommandParameter is not OldDosQueueItemViewModel item ||
           DataContext is not AdminOldDosImportsViewModel vm) return;

        string url = vm.AbsoluteOldDosUrl(item.SourceUrl);
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
