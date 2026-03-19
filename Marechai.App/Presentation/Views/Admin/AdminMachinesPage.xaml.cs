using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminMachinesPage : Page
{
    public AdminMachinesPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is AdminMachinesViewModel vm && vm.IsAdmin)
            _ = vm.LoadPickerDataAsync();
    }

    private void CompanyFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminMachinesViewModel vm)
            vm.UpdateCompanySuggestions(sender.Text);
    }

    private void FilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(DataContext is AdminMachinesViewModel vm)
            vm.ApplyFilter();
    }
}
