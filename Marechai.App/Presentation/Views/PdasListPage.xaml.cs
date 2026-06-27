using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

/// <summary>
///     Professional list view for displaying PDAs filtered by letter, year, or all.
///     Features responsive layout, modern styling, and smooth navigation.
/// </summary>
public sealed partial class PdasListPage : Page
{
    public PdasListPage()
    {
        InitializeComponent();
        Loaded             += PdasListPage_Loaded;
        DataContextChanged += PdasListPage_DataContextChanged;
    }

    private void PdasListPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is PdasListViewModel vm)
        {
            // Load data when DataContext is set
            vm.LoadData.Execute(null);
        }
    }

    private void PdasListPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is PdasListViewModel vm)
        {
            // Load data when page is loaded (fallback)
            vm.LoadData.Execute(null);
        }
    }
}
