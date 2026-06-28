using System;
using System.ComponentModel;
using Marechai.ApiClient.Models;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class MainPage : Page
{
    private PropertyChangedEventHandler _sidebarPropertyChangedHandler;

    public MainPage()
    {
        InitializeComponent();
        DataContextChanged += MainPage_DataContextChanged;
        Loaded             += MainPage_Loaded;
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is not MainViewModel viewModel) return;

        SidebarWrapper.Width = viewModel.IsSidebarOpen ? 280 : 60;
        _ = viewModel.InitializeMessagingAsync();

        if(_sidebarPropertyChangedHandler != null) return;

        _sidebarPropertyChangedHandler = (_, propArgs) =>
        {
            if(propArgs.PropertyName != nameof(MainViewModel.IsSidebarOpen)) return;

            AnimateSidebarWidth(((MainViewModel)DataContext).IsSidebarOpen);
        };

        ((INotifyPropertyChanged)viewModel).PropertyChanged += _sidebarPropertyChangedHandler;
    }

    void AnimateSidebarWidth(bool isOpen)
    {
        double start = SidebarColumn.Width.Value;
        double end   = isOpen ? 280 : 60;

        if(Math.Abs(start - end) < 0.1) return;

        // If expanding, show content immediately
        if(isOpen && DataContext is MainViewModel vm) vm.SidebarContentVisible = true;

        const int durationMs  = 250;
        const int fps         = 60;
        var       steps       = (int)(durationMs / (1000.0 / fps));
        var       currentStep = 0;

        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000.0 / fps)
        };

        timer.Tick += (_, _) =>
        {
            currentStep++;
            double t = (double)currentStep / steps;

            // Ease in-out cubic
            double eased = t < 0.5 ? 4           * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;
            double value = start + (end - start) * eased;
            SidebarColumn.Width  = new GridLength(value, GridUnitType.Pixel);
            SidebarWrapper.Width = value;

            if(currentStep >= steps)
            {
                SidebarColumn.Width  = new GridLength(end, GridUnitType.Pixel);
                SidebarWrapper.Width = end;
                timer.Stop();

                // After collapse animation completes, hide sidebar content
                if(!isOpen && DataContext is MainViewModel vm) vm.SidebarContentVisible = false;
            }
        };

        timer.Start();
    }

    private void MainPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is MainViewModel vm && _sidebarPropertyChangedHandler == null)
        {
            SidebarWrapper.Width = vm.IsSidebarOpen ? 280 : 60;

            _sidebarPropertyChangedHandler = (_, propArgs) =>
            {
                if(propArgs.PropertyName != nameof(MainViewModel.IsSidebarOpen)) return;
                AnimateSidebarWidth(vm.IsSidebarOpen);
            };

            ((INotifyPropertyChanged)vm).PropertyChanged += _sidebarPropertyChangedHandler;
        }
    }

    private void OnGlobalSearchSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if(DataContext is not MainViewModel viewModel) return;

        if(args.SelectedItem is SearchResultDto result) viewModel.NavigateToSearchResultCommand.Execute(result);
    }

    private void OnGlobalSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if(DataContext is not MainViewModel viewModel) return;

        if(args.ChosenSuggestion is SearchResultDto result)
            viewModel.NavigateToSearchResultCommand.Execute(result);
        else
            viewModel.SubmitGlobalSearchCommand.Execute(null);
    }
}
