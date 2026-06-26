using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminExternalSitesPage : Page
{
    public AdminExternalSitesPage() => InitializeComponent();

    private void FilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(DataContext is AdminExternalSitesViewModel vm)
            vm.ApplyFilter();
    }
}
