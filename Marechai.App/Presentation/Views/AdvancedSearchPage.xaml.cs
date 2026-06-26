using Marechai.ApiClient.Models;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class AdvancedSearchPage : Page
{
    public AdvancedSearchPage() => InitializeComponent();

    private void OnCompanySuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if(DataContext is not AdvancedSearchViewModel viewModel) return;
        if(args.SelectedItem is not SearchResultDto result) return;

        viewModel.SelectedCompany   = result;
        viewModel.CompanySearchText = result.DisplayName;
    }

    private void OnIncludesSoftwareSuggestionChosen(AutoSuggestBox sender,
                                                    AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if(DataContext is not AdvancedSearchViewModel viewModel) return;
        if(args.SelectedItem is not SearchResultDto result) return;

        viewModel.SelectedIncludesSoftware    = result;
        viewModel.IncludesSoftwareSearchText  = result.DisplayName;
    }

    private void OnScrollViewerViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        if(e.IsIntermediate) return;
        if(sender is not ScrollViewer scroller) return;
        if(DataContext is not AdvancedSearchViewModel viewModel) return;

        double distanceToEnd = scroller.ExtentHeight - (scroller.VerticalOffset + scroller.ViewportHeight);

        if(distanceToEnd <= 2.0 * scroller.ViewportHeight) _ = viewModel.LoadMoreCommand.ExecuteAsync(null);
    }
}
