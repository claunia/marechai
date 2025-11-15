using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Marechai.App.Presentation.Views;

public sealed partial class CompaniesPage : Page
{
    public CompaniesPage()
    {
        InitializeComponent();
        DataContextChanged += CompaniesPage_DataContextChanged;
        Loaded             += CompaniesPage_Loaded;
    }

    private void CompaniesPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is not CompaniesViewModel viewModel) return;

        // Trigger data loading
        _ = viewModel.LoadData.ExecuteAsync(null);
    }

    private void CompaniesPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is CompaniesViewModel viewModel)
        {
            // Trigger data loading when data context changes
            _ = viewModel.LoadData.ExecuteAsync(null);
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if(DataContext is CompaniesViewModel viewModel)
        {
            // Trigger data loading when navigating to the page
            _ = viewModel.LoadData.ExecuteAsync(null);
        }
    }

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            // The two-way binding will automatically update SearchQuery in ViewModel,
            // which will trigger OnSearchQueryChanged and filter the list
        }
    }

    private void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        // The two-way binding will automatically update SearchQuery in ViewModel,
        // which will trigger OnSearchQueryChanged and filter the list
    }
}