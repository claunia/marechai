using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Marechai.App.Presentation.Views;

public sealed partial class SmartphonesPage : Page
{
    public SmartphonesPage()
    {
        InitializeComponent();
        DataContextChanged += SmartphonesPage_DataContextChanged;
        Loaded             += SmartphonesPage_Loaded;
    }

    private void SmartphonesPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is not SmartphonesViewModel viewModel) return;

        // Trigger data loading
        _ = viewModel.LoadData.ExecuteAsync(null);
    }

    private void SmartphonesPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is SmartphonesViewModel viewModel)
        {
            // Trigger data loading when data context changes
            _ = viewModel.LoadData.ExecuteAsync(null);
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if(DataContext is SmartphonesViewModel viewModel)
        {
            // Trigger data loading when navigating to the page
            _ = viewModel.LoadData.ExecuteAsync(null);
        }
    }
}
