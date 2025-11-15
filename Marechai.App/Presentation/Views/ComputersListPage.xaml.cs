using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

/// <summary>
///     Professional list view for displaying computers filtered by letter, year, or all.
///     Features responsive layout, modern styling, and smooth navigation.
/// </summary>
public sealed partial class ComputersListPage : Page
{
    public ComputersListPage()
    {
        InitializeComponent();
        Loaded             += ComputersListPage_Loaded;
        DataContextChanged += ComputersListPage_DataContextChanged;
    }

    private void ComputersListPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is ComputersListViewModel vm)
        {
            // Load data when DataContext is set
            vm.LoadData.Execute(null);
        }
    }

    private void ComputersListPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is ComputersListViewModel vm)
        {
            // Load data when page is loaded (fallback)
            vm.LoadData.Execute(null);
        }
    }
}