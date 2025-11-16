using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

/// <summary>
///     Professional list view for displaying all graphics processing units.
///     Features responsive layout, modern styling, and special handling for Framebuffer and Software entries.
/// </summary>
public sealed partial class GpuListPage : Page
{
    public GpuListPage()
    {
        InitializeComponent();
        Loaded             += GpuListPage_Loaded;
        DataContextChanged += GpuListPage_DataContextChanged;
    }

    private void GpuListPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is GpusListViewModel vm)
        {
            // Load data when DataContext is set
            vm.LoadData.Execute(null);
        }
    }

    private void GpuListPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is GpusListViewModel vm)
        {
            // Load data when page is loaded (fallback)
            vm.LoadData.Execute(null);
        }
    }
}