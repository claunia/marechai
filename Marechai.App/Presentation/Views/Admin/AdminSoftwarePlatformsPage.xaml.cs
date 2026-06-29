using System;
using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminSoftwarePlatformsPage : Page
{
    public AdminSoftwarePlatformsPage() => InitializeComponent();

    private void FilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(DataContext is AdminSoftwarePlatformsViewModel vm)
            vm.ApplyFilter();
    }

    private void MergeTargetFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwarePlatformsViewModel vm)
            vm.UpdateMergeTargetSuggestions(sender.Text);
    }

    private async void MergeButton_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is not AdminSoftwarePlatformsViewModel vm) return;

        var dialog = new ContentDialog
        {
            XamlRoot          = XamlRoot,
            Title             = vm.MergeConfirmDialogTitle,
            PrimaryButtonText = vm.MergeButtonText,
            CloseButtonText   = vm.CancelButtonText,
            DefaultButton     = ContentDialogButton.Close,
            Content           = vm.MergeConfirmDialogMessage
        };

        ContentDialogResult result = await dialog.ShowAsync();
        if(result != ContentDialogResult.Primary) return;

        await vm.ConfirmMergeCommand.ExecuteAsync(null);
    }
}
