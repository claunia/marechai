#nullable enable

using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Marechai.App.Presentation.Views;

/// <summary>
///     Processor detail page showing all information about a specific processor
/// </summary>
public sealed partial class ProcessorDetailPage : Page
{
    private object? _navigationSource;
    private int?    _pendingProcessorId;

    public ProcessorDetailPage()
    {
        InitializeComponent();
        DataContextChanged += ProcessorDetailPage_DataContextChanged;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        int? processorId = null;

        // Handle both int and ProcessorDetailNavigationParameter
        if(e.Parameter is int intId)
            processorId = intId;
        else if(e.Parameter is ProcessorDetailNavigationParameter navParam)
        {
            processorId      = navParam.ProcessorId;
            _navigationSource = navParam.NavigationSource;
        }

        if(processorId.HasValue)
        {
            _pendingProcessorId = processorId;

            if(DataContext is ProcessorDetailViewModel viewModel)
            {
                viewModel.ProcessorId = processorId.Value;
                if(_navigationSource != null) viewModel.SetNavigationSource(_navigationSource);
                _ = viewModel.LoadData.ExecuteAsync(null);
            }
        }
    }

    private void ProcessorDetailPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(_pendingProcessorId.HasValue && DataContext is ProcessorDetailViewModel viewModel)
        {
            viewModel.ProcessorId = _pendingProcessorId.Value;
            if(_navigationSource != null) viewModel.SetNavigationSource(_navigationSource);
            _ = viewModel.LoadData.ExecuteAsync(null);
        }
    }

    private void ComputersSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if(DataContext is ProcessorDetailViewModel vm) vm.ComputersFilterCommand.Execute(null);
    }

    private void ComputersSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            if(DataContext is ProcessorDetailViewModel vm)
                vm.ComputersFilterCommand.Execute(null);
    }

    private void ConsolesSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if(DataContext is ProcessorDetailViewModel vm) vm.ConsolesFilterCommand.Execute(null);
    }

    private void ConsolesSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            if(DataContext is ProcessorDetailViewModel vm)
                vm.ConsolesFilterCommand.Execute(null);
    }

    private void Computer_Click(object sender, RoutedEventArgs e)
    {
        if(sender is Button button && button.Tag is int machineId && DataContext is ProcessorDetailViewModel vm)
            _ = vm.SelectMachineCommand.ExecuteAsync(machineId);
    }

    private void Console_Click(object sender, RoutedEventArgs e)
    {
        if(sender is Button button && button.Tag is int machineId && DataContext is ProcessorDetailViewModel vm)
            _ = vm.SelectMachineCommand.ExecuteAsync(machineId);
    }
}
