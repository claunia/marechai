#nullable enable

using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Marechai.App.Presentation.Views;

/// <summary>
///     GPU detail page showing all information, resolutions, computers, and consoles
/// </summary>
public sealed partial class GpuDetailPage : Page
{
    private object? _navigationSource;
    private int?    _pendingGpuId;

    public GpuDetailPage()
    {
        InitializeComponent();
        DataContextChanged += GpuDetailPage_DataContextChanged;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        int? gpuId = null;

        // Handle both int and GpuDetailNavigationParameter
        if(e.Parameter is int intId)
            gpuId = intId;
        else if(e.Parameter is GpuDetailNavigationParameter navParam)
        {
            gpuId             = navParam.GpuId;
            _navigationSource = navParam.NavigationSource;
        }

        if(gpuId.HasValue)
        {
            _pendingGpuId = gpuId;

            if(DataContext is GpuDetailViewModel viewModel)
            {
                viewModel.GpuId = gpuId.Value;
                if(_navigationSource != null) viewModel.SetNavigationSource(_navigationSource);
                _ = viewModel.LoadData.ExecuteAsync(null);
            }
        }
    }

    private void GpuDetailPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is GpuDetailViewModel viewModel && _pendingGpuId.HasValue)
        {
            viewModel.GpuId = _pendingGpuId.Value;
            if(_navigationSource != null) viewModel.SetNavigationSource(_navigationSource);
            _ = viewModel.LoadData.ExecuteAsync(null);
        }
    }

    private void ComputersSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if(DataContext is GpuDetailViewModel vm) vm.ComputersFilterCommand.Execute(null);
    }

    private void ComputersSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            if(DataContext is GpuDetailViewModel vm)
                vm.ComputersFilterCommand.Execute(null);
    }

    private void ConsolesSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if(DataContext is GpuDetailViewModel vm) vm.ConsolesFilterCommand.Execute(null);
    }

    private void ConsolesSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            if(DataContext is GpuDetailViewModel vm)
                vm.ConsolesFilterCommand.Execute(null);
    }

    private void Computer_Click(object sender, RoutedEventArgs e)
    {
        if(sender is Button button && button.Tag is int machineId && DataContext is GpuDetailViewModel vm)
            _ = vm.SelectMachineCommand.ExecuteAsync(machineId);
    }

    private void Console_Click(object sender, RoutedEventArgs e)
    {
        if(sender is Button button && button.Tag is int machineId && DataContext is GpuDetailViewModel vm)
            _ = vm.SelectMachineCommand.ExecuteAsync(machineId);
    }
}