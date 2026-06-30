using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminProcessorsPage : Page
{
    public AdminProcessorsPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is AdminProcessorsViewModel vm && vm.IsAdmin)
            _ = vm.LoadPickerDataAsync();
    }

    private void CompanyFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminProcessorsViewModel vm)
            vm.UpdateCompanySuggestions(sender.Text);
    }

    private void FilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminProcessorsViewModel vm)
        {
            vm.FilterText = sender.Text;
            vm.ApplyFilter();
        }
    }

    private async void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(DataContext is not AdminProcessorsViewModel vm) return;

        vm.CurrentPage = 1;
        await vm.LoadProcessorsCommand.ExecuteAsync(null);
    }
}
