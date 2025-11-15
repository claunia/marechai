using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

/// <summary>
///     Professional list view for displaying consoles filtered by letter, year, or all.
///     Features responsive layout, modern styling, and smooth navigation.
/// </summary>
public sealed partial class ConsolesListPage : Page
{
    public ConsolesListPage()
    {
        InitializeComponent();
        Loaded             += ConsolesListPage_Loaded;
        DataContextChanged += ConsolesListPage_DataContextChanged;
    }

    private void ConsolesListPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is ConsolesListViewModel vm)
        {
            // Load data when DataContext is set
            vm.LoadData.Execute(null);
        }
    }

    private void ConsolesListPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is ConsolesListViewModel vm)
        {
            // Load data when page is loaded (fallback)
            vm.LoadData.Execute(null);
        }
    }
}