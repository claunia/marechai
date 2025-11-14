using System;
using System.ComponentModel;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Marechai.App.Presentation;

public sealed partial class MainPage : Page
{
    private bool                        _initialNewsLoaded;
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

        if(_initialNewsLoaded) return;

        if(args.NewValue is MainViewModel viewModel && viewModel.NewsViewModel is not null)
        {
            _initialNewsLoaded =  true;
            _                  =  viewModel.NewsViewModel.LoadNews.ExecuteAsync(null);
            DataContextChanged -= MainPage_DataContextChanged;
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if(_initialNewsLoaded) return;

        if(DataContext is MainViewModel viewModel && viewModel.NewsViewModel is not null)
        {
            _initialNewsLoaded =  true;
            _                  =  viewModel.NewsViewModel.LoadNews.ExecuteAsync(null);
            DataContextChanged -= MainPage_DataContextChanged;
        }
    }

    private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
    {
        // Handle pull-to-refresh
        using Deferral deferral = args.GetDeferral();

        try
        {
            if(DataContext is MainViewModel viewModel && viewModel.NewsViewModel is not null)
                await viewModel.NewsViewModel.LoadNews.ExecuteAsync(null);
        }
        catch(Exception)
        {
            // Swallow to avoid process crash; NewsViewModel already logs errors.
        }
    }
}