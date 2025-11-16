#nullable enable

using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Marechai.App.Presentation.Views;

/// <summary>
///     Sound Synthesizer detail page showing all information, computers, and consoles
/// </summary>
public sealed partial class SoundSynthDetailPage : Page
{
    private object? _navigationSource;
    private int?    _pendingSoundSynthId;

    public SoundSynthDetailPage()
    {
        InitializeComponent();
        DataContextChanged += SoundSynthDetailPage_DataContextChanged;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        int? soundSynthId = null;

        // Handle both int and SoundSynthDetailNavigationParameter
        if(e.Parameter is int intId)
            soundSynthId = intId;
        else if(e.Parameter is SoundSynthDetailNavigationParameter navParam)
        {
            soundSynthId      = navParam.SoundSynthId;
            _navigationSource = navParam.NavigationSource;
        }

        if(soundSynthId.HasValue)
        {
            _pendingSoundSynthId = soundSynthId;

            if(DataContext is SoundSynthDetailViewModel viewModel)
            {
                viewModel.SoundSynthId = soundSynthId.Value;
                if(_navigationSource != null) viewModel.SetNavigationSource(_navigationSource);
                _ = viewModel.LoadData.ExecuteAsync(null);
            }
        }
    }

    private void SoundSynthDetailPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is SoundSynthDetailViewModel viewModel && _pendingSoundSynthId.HasValue)
        {
            viewModel.SoundSynthId = _pendingSoundSynthId.Value;
            if(_navigationSource != null) viewModel.SetNavigationSource(_navigationSource);
            _ = viewModel.LoadData.ExecuteAsync(null);
        }
    }

    private void ComputersSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if(DataContext is SoundSynthDetailViewModel vm) vm.ComputersFilterCommand.Execute(null);
    }

    private void ComputersSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            if(DataContext is SoundSynthDetailViewModel vm) vm.ComputersFilterCommand.Execute(null);
        }
    }

    private void ConsolesSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if(DataContext is SoundSynthDetailViewModel vm) vm.ConsolesFilterCommand.Execute(null);
    }

    private void ConsolesSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            if(DataContext is SoundSynthDetailViewModel vm) vm.ConsolesFilterCommand.Execute(null);
        }
    }

    private void Computer_Click(object sender, RoutedEventArgs e)
    {
        if(sender is Button button && button.Tag is int machineId && DataContext is SoundSynthDetailViewModel vm)
            _ = vm.SelectMachineCommand.ExecuteAsync(machineId);
    }

    private void Console_Click(object sender, RoutedEventArgs e)
    {
        if(sender is Button button && button.Tag is int machineId && DataContext is SoundSynthDetailViewModel vm)
            _ = vm.SelectMachineCommand.ExecuteAsync(machineId);
    }
}