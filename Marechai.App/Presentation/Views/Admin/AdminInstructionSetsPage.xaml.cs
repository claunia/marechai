using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminInstructionSetsPage : Page
{
    public AdminInstructionSetsPage()
    {
        InitializeComponent();
    }

    private void FilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(DataContext is AdminInstructionSetsViewModel vm)
            vm.ApplyFilter();
    }

    private void NameBox_TextChanged(object sender, TextChangedEventArgs args)
    {
        if(DataContext is AdminInstructionSetsViewModel vm)
            _ = vm.VerifyNameUniquenessAsync();
    }
}
