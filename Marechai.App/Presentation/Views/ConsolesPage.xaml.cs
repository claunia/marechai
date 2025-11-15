using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Marechai.App.Presentation.Views;

public sealed partial class ConsolesPage : Page
{
    public ConsolesPage()
    {
        InitializeComponent();
        DataContextChanged += ConsolesPage_DataContextChanged;
        Loaded             += ConsolesPage_Loaded;
    }

    private void ConsolesPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is not ConsolesViewModel viewModel) return;

        // Trigger data loading
        _ = viewModel.LoadData.ExecuteAsync(null);
    }

    private void ConsolesPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is ConsolesViewModel viewModel)
        {
            // Trigger data loading when data context changes
            _ = viewModel.LoadData.ExecuteAsync(null);
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if(DataContext is ConsolesViewModel viewModel)
        {
            // Trigger data loading when navigating to the page
            _ = viewModel.LoadData.ExecuteAsync(null);
        }
    }
}