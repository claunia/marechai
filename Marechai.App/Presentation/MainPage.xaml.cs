using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Marechai.App.Presentation;

public sealed partial class MainPage : Page
{
    private bool _initialNewsLoaded;

    public MainPage()
    {
        InitializeComponent();
        DataContextChanged += MainPage_DataContextChanged;
    }

    private void MainPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
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