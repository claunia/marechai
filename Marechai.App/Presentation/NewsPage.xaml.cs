using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Marechai.App.Presentation;

public sealed partial class NewsPage : Page
{
    private bool _initialNewsLoaded;

    public NewsPage()
    {
        InitializeComponent();
        DataContextChanged += NewsPage_DataContextChanged;
        Loaded             += NewsPage_Loaded;
    }

    private void NewsPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(_initialNewsLoaded) return;

        if(DataContext is NewsViewModel viewModel)
        {
            _initialNewsLoaded =  true;
            _                  =  viewModel.LoadNews.ExecuteAsync(null);
            DataContextChanged -= NewsPage_DataContextChanged;
        }
    }

    private void NewsPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(_initialNewsLoaded) return;

        if(args.NewValue is NewsViewModel viewModel)
        {
            _initialNewsLoaded =  true;
            _                  =  viewModel.LoadNews.ExecuteAsync(null);
            DataContextChanged -= NewsPage_DataContextChanged;
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if(_initialNewsLoaded) return;

        if(DataContext is NewsViewModel viewModel)
        {
            _initialNewsLoaded =  true;
            _                  =  viewModel.LoadNews.ExecuteAsync(null);
            DataContextChanged -= NewsPage_DataContextChanged;
        }
    }

    private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
    {
        // Handle pull-to-refresh
        using Deferral deferral = args.GetDeferral();

        try
        {
            if(DataContext is NewsViewModel viewModel) await viewModel.LoadNews.ExecuteAsync(null);
        }
        catch(Exception)
        {
            // Swallow to avoid process crash; NewsViewModel already logs errors.
        }
    }
}