using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminDocumentsPage : Page
{
    public AdminDocumentsPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is AdminDocumentsViewModel vm && vm.IsAdmin)
            _ = vm.LoadPickerDataAsync();
    }

    private void FilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(DataContext is AdminDocumentsViewModel vm)
            vm.ApplyFilter();
    }

    private async void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(DataContext is not AdminDocumentsViewModel vm) return;

        vm.CurrentPage = 1;
        await vm.LoadDocumentsCommand.ExecuteAsync(null);
    }

    private void PersonSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminDocumentsViewModel vm)
            vm.UpdatePeopleSuggestions(sender.Text);
    }

    private void CompanySearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminDocumentsViewModel vm)
            vm.UpdateCompanySuggestions(sender.Text);
    }
}
