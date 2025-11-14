using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Marechai.App.Presentation;

public sealed partial class ComputersPage : Page
{
    public ComputersPage()
    {
        InitializeComponent();
        DataContextChanged += ComputersPage_DataContextChanged;
        Loaded             += ComputersPage_Loaded;
    }

    private void ComputersPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is not ComputersViewModel viewModel) return;

        // Trigger data loading
        _ = viewModel.LoadData.ExecuteAsync(null);
    }

    private void ComputersPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is ComputersViewModel viewModel)
        {
            // Trigger data loading when data context changes
            _ = viewModel.LoadData.ExecuteAsync(null);
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if(DataContext is ComputersViewModel viewModel)
        {
            // Trigger data loading when navigating to the page
            _ = viewModel.LoadData.ExecuteAsync(null);
        }
    }
}