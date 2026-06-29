using Marechai.ApiClient.Models;
using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminSoftwareCompilationsPage : Page
{
    public AdminSoftwareCompilationsPage()
    {
        InitializeComponent();
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwareCompilationsViewModel vm)
            _ = vm.SearchAsync();
    }

    private void SoftwareFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwareCompilationsViewModel vm)
            vm.UpdateSoftwareSuggestions(sender.Text);
    }

    private void MachineFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwareCompilationsViewModel vm)
            vm.UpdateMachineSuggestions(sender.Text);
    }

    private void PredecessorFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwareCompilationsViewModel vm)
            vm.UpdatePredecessorSuggestions(sender.Text);
    }

    private void IncludedSoftwareFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwareCompilationsViewModel vm)
            vm.UpdateIncludedSoftwareSuggestions(sender.Text);
    }

    private void IncludedCompilationFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
           DataContext is AdminSoftwareCompilationsViewModel vm)
            vm.UpdateIncludedCompilationSuggestions(sender.Text);
    }

    private void VersionSoftwarePicker_SelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if(DataContext is AdminSoftwareCompilationsViewModel vm && sender is ComboBox { SelectedItem: SoftwareDto software })
            vm.SelectSoftwareForVersions(software);
    }
}
