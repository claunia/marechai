using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminSoundSynthsPage : Page
{
    public AdminSoundSynthsPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is AdminSoundSynthsViewModel vm && vm.IsAdmin)
            _ = vm.LoadPickerDataAsync();
    }

    private void CompanyFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoundSynthsViewModel vm)
            vm.UpdateCompanySuggestions(sender.Text);
    }

    private void FilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(DataContext is AdminSoundSynthsViewModel vm)
            vm.ApplyFilter();
    }

    private async void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(DataContext is not AdminSoundSynthsViewModel vm) return;

        vm.CurrentPage = 1;
        await vm.LoadItemsCommand.ExecuteAsync(null);
    }
}
