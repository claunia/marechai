using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminInstructionSetExtensionsPage : Page
{
    public AdminInstructionSetExtensionsPage()
    {
        InitializeComponent();
    }

    private void FilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(DataContext is AdminInstructionSetExtensionsViewModel vm)
            vm.ApplyFilter();
    }

    private void NameBox_TextChanged(object sender, TextChangedEventArgs args)
    {
        if(DataContext is AdminInstructionSetExtensionsViewModel vm)
            _ = vm.VerifyNameUniquenessAsync();
    }
}
