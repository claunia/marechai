using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

/// <summary>
///     Professional list view for displaying Tablets filtered by letter, year, or all.
///     Features responsive layout, modern styling, and smooth navigation.
/// </summary>
public sealed partial class TabletsListPage : Page
{
    public TabletsListPage()
    {
        InitializeComponent();
        Loaded             += TabletsListPage_Loaded;
        DataContextChanged += TabletsListPage_DataContextChanged;
    }

    private void TabletsListPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is TabletsListViewModel vm)
        {
            // Load data when DataContext is set
            vm.LoadData.Execute(null);
        }
    }

    private void TabletsListPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is TabletsListViewModel vm)
        {
            // Load data when page is loaded (fallback)
            vm.LoadData.Execute(null);
        }
    }
}
