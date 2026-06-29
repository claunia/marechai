using System;
using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminSoftwareDuplicatesPage : Page
{
    public AdminSoftwareDuplicatesPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is AdminSoftwareDuplicatesViewModel vm && vm.IsAdmin)
            _ = vm.LoadPickerDataAsync();
    }

    private void MasterToggle_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is not AdminSoftwareDuplicatesViewModel vm) return;
        if(sender is not FrameworkElement { DataContext: DuplicateItemRow row }) return;

        vm.SetMasterCommand.Execute(row);
    }

    private void OpenInNewWindow_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is not AdminSoftwareDuplicatesViewModel vm) return;
        if(sender is not FrameworkElement { DataContext: DuplicateItemRow row }) return;

        vm.OpenInNewWindowCommand.Execute(row);
    }

    private void MergeIntoMaster_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is not AdminSoftwareDuplicatesViewModel vm) return;
        if(sender is not FrameworkElement { DataContext: DuplicateItemRow row }) return;

        _ = vm.OpenMergeIntoMasterCommand.ExecuteAsync(row);
    }

    private void MergeIntoOther_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is not AdminSoftwareDuplicatesViewModel vm) return;
        if(sender is not FrameworkElement { DataContext: DuplicateItemRow row }) return;

        vm.OpenMergeCommand.Execute(row);
    }

    private void MergeTargetFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwareDuplicatesViewModel vm)
            vm.UpdateMergeTargetSuggestions(sender.Text);
    }

    private async void MergeButton_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is not AdminSoftwareDuplicatesViewModel vm) return;

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
