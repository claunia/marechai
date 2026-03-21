using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminSoftwareVariantsPage : Page
{
    public AdminSoftwareVariantsPage() => InitializeComponent();

    private void FilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(DataContext is AdminSoftwareVariantsViewModel vm) vm.ApplyFilter();
    }
}
