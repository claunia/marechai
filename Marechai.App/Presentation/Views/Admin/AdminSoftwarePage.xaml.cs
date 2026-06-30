using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminSoftwarePage : Page
{
    public AdminSoftwarePage()
    {
        InitializeComponent();
    }

    private void FilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && DataContext is AdminSoftwareViewModel vm)
            vm.ApplyFilter();
    }

    private async void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(DataContext is not AdminSoftwareViewModel vm) return;

        vm.CurrentPage = 1;
        await vm.LoadCommand.ExecuteAsync(null);
    }

    private void FamilyFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && DataContext is AdminSoftwareViewModel vm)
            vm.UpdateFamilySuggestions(sender.Text);
    }

    private async void SimilarSoftwareFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && DataContext is AdminSoftwareViewModel vm)
            await vm.UpdateSimilarSoftwareSuggestionsAsync(sender.Text);
    }
}
