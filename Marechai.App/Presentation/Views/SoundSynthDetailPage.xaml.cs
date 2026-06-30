#nullable enable

using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class SoundSynthDetailPage : Page
{
    public SoundSynthDetailPage()
    {
        InitializeComponent();
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