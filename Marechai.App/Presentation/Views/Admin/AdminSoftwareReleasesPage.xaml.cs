using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminSoftwareReleasesPage : Page
{
    public AdminSoftwareReleasesPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is AdminSoftwareReleasesViewModel vm && vm.IsAdmin)
            _ = vm.LoadPickerDataAsync();
    }

    private void FilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(DataContext is AdminSoftwareReleasesViewModel vm) vm.ApplyFilter();
    }

    private void VersionFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && DataContext is AdminSoftwareReleasesViewModel vm)
            vm.UpdateVersionSuggestions(sender.Text);
    }

    private void RegionFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && DataContext is AdminSoftwareReleasesViewModel vm)
            vm.UpdateRegionSuggestions(sender.Text);
    }

    private void PublisherFilterBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && DataContext is AdminSoftwareReleasesViewModel vm)
            vm.UpdatePublisherSuggestions(sender.Text);
    }

    private void MinGpuSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && DataContext is AdminSoftwareReleasesViewModel vm)
            vm.UpdateMinimumGpuSuggestions(sender.Text);
    }

    private void RecGpuSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && DataContext is AdminSoftwareReleasesViewModel vm)
            vm.UpdateRecommendedGpuSuggestions(sender.Text);
    }

    private void SoundSynthSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && DataContext is AdminSoftwareReleasesViewModel vm)
            vm.UpdateSoundSynthSuggestions(sender.Text);
    }
}
