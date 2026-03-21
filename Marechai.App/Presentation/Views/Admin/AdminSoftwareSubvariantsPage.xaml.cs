using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminSoftwareSubvariantsPage : Page
{
    public AdminSoftwareSubvariantsPage() => InitializeComponent();

    private void FilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(DataContext is AdminSoftwareSubvariantsViewModel vm) vm.ApplyFilter();
    }
}
