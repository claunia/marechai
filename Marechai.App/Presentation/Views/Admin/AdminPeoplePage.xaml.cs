using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminPeoplePage : Page
{
    public AdminPeoplePage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is AdminPeopleViewModel vm && vm.IsAdmin)
            _ = vm.LoadPickerDataAsync();
    }

    private void FilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(DataContext is AdminPeopleViewModel vm)
            vm.ApplyFilter();
    }

    private async void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(DataContext is not AdminPeopleViewModel vm) return;

        vm.CurrentPage = 1;
        await vm.LoadPeopleCommand.ExecuteAsync(null);
    }
}
