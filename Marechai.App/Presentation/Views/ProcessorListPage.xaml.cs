using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

/// <summary>
///     Professional list view for displaying all processors.
///     Features responsive layout and modern styling.
/// </summary>
public sealed partial class ProcessorListPage : Page
{
    public ProcessorListPage()
    {
        InitializeComponent();
        Loaded             += ProcessorListPage_Loaded;
        DataContextChanged += ProcessorListPage_DataContextChanged;
    }

    private void ProcessorListPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is ProcessorsListViewModel vm)
        {
            // Load data when DataContext is set
            vm.LoadData.Execute(null);
        }
    }

    private void ProcessorListPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is ProcessorsListViewModel vm)
        {
            // Load data when page is loaded (fallback)
            vm.LoadData.Execute(null);
        }
    }
}
