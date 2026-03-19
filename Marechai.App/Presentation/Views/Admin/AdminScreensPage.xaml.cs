using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminScreensPage : Page
{
    public AdminScreensPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is AdminScreensViewModel vm && vm.IsAdmin)
            _ = vm.LoadPickerDataAsync();
    }

    private void FilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(DataContext is AdminScreensViewModel vm)
            vm.ApplyFilter();
    }

    private void ResolutionFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminScreensViewModel vm)
            vm.UpdateResolutionFilter(sender.Text);
    }
}
