using System;
using Marechai.ApiClient.Models;
using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminSoftwareCompilationsPage : Page
{
    public AdminSoftwareCompilationsPage()
    {
        InitializeComponent();
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwareCompilationsViewModel vm)
            _ = vm.SearchAsync();
    }

    private void SoftwareFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwareCompilationsViewModel vm)
            vm.UpdateSoftwareSuggestions(sender.Text);
    }

    private void MachineFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwareCompilationsViewModel vm)
            vm.UpdateMachineSuggestions(sender.Text);
    }

    private void PredecessorFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwareCompilationsViewModel vm)
            vm.UpdatePredecessorSuggestions(sender.Text);
    }

    private void IncludedSoftwareFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwareCompilationsViewModel vm)
            vm.UpdateIncludedSoftwareSuggestions(sender.Text);
    }

    private void IncludedCompilationFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwareCompilationsViewModel vm)
            vm.UpdateIncludedCompilationSuggestions(sender.Text);
    }

    private void VersionSoftwarePicker_SelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if(DataContext is AdminSoftwareCompilationsViewModel vm && sender is ComboBox { SelectedItem: SoftwareDto software })
            vm.SelectSoftwareForVersions(software);
    }

    private void MergeTargetFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwareCompilationsViewModel vm)
            vm.UpdateMergeTargetSuggestions(sender.Text);
    }

    private void MergeSourceFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwareCompilationsViewModel vm)
            vm.UpdateMergeSourceSuggestions(sender.Text);
    }

    private async void MergeButton_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is not AdminSoftwareCompilationsViewModel vm) return;

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
