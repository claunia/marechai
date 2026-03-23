using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminSoftwareVersionsPage : Page
{
    public AdminSoftwareVersionsPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is AdminSoftwareVersionsViewModel vm && vm.IsAdmin)
            _ = vm.LoadPickerDataAsync();
    }

    private void FilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(DataContext is AdminSoftwareVersionsViewModel vm) vm.ApplyFilter();
    }

    private void ParentVersionFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && DataContext is AdminSoftwareVersionsViewModel vm)
            vm.UpdateParentVersionSuggestions(sender.Text);
    }

    private void CompanySearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && DataContext is AdminSoftwareVersionsViewModel vm)
            vm.UpdateCompanySuggestions(sender.Text);
    }
}
