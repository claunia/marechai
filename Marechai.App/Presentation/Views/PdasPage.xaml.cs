using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Marechai.App.Presentation.Views;

public sealed partial class PdasPage : Page
{
    public PdasPage()
    {
        InitializeComponent();
        DataContextChanged += PdasPage_DataContextChanged;
        Loaded             += PdasPage_Loaded;
    }

    private void PdasPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is not PdasViewModel viewModel) return;

        // Trigger data loading
        _ = viewModel.LoadData.ExecuteAsync(null);
    }

    private void PdasPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is PdasViewModel viewModel)
        {
            // Trigger data loading when data context changes
            _ = viewModel.LoadData.ExecuteAsync(null);
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if(DataContext is PdasViewModel viewModel)
        {
            // Trigger data loading when navigating to the page
            _ = viewModel.LoadData.ExecuteAsync(null);
        }
    }
}
