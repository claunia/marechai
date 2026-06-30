using System;
using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminCompaniesPage : Page
{
    public AdminCompaniesPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is AdminCompaniesViewModel vm && vm.IsAdmin)
            _ = vm.LoadPickerDataAsync();
    }

    private void SoldToFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminCompaniesViewModel vm)
            vm.UpdateSoldToSuggestions(sender.Text);
    }

    private void FilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(DataContext is AdminCompaniesViewModel vm)
            vm.ApplyFilter();
    }

    private async void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(DataContext is not AdminCompaniesViewModel vm) return;

        vm.CurrentPage = 1;
        await vm.LoadCompaniesCommand.ExecuteAsync(null);
    }

    private void PersonSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminCompaniesViewModel vm)
            vm.UpdatePeopleSuggestions(sender.Text);
    }

    private void MergeTargetFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminCompaniesViewModel vm)
            vm.UpdateMergeTargetSuggestions(sender.Text);
    }

    private async void MergeButton_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is not AdminCompaniesViewModel vm) return;

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
