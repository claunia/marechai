using System.ComponentModel;
using Marechai.ApiClient.Models;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Marechai.App.Presentation.Views;

public sealed partial class MainPage : Page
{
    private const double ExpandedSidebarWidth = 280;
    private const double CompactSidebarWidth  = 60;

    private INotifyPropertyChanged? _observedViewModel;
    private bool _isWideLayout;

    public MainPage()
    {
        InitializeComponent();
        DataContextChanged += MainPage_DataContextChanged;
        Loaded             += MainPage_Loaded;
        SizeChanged        += MainPage_SizeChanged;
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is not MainViewModel viewModel) return;

        _ = viewModel.InitializeMessagingAsync();
        UpdateResponsiveMode(viewModel);
        UpdateShellLayout(viewModel);
    }

    private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if(DataContext is not MainViewModel viewModel) return;

        UpdateResponsiveMode(viewModel);
        UpdateShellLayout(viewModel);
    }

    private void MainPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(_observedViewModel is not null)
            _observedViewModel.PropertyChanged -= OnViewModelPropertyChanged;

        if(args.NewValue is not MainViewModel vm) return;

        _observedViewModel = vm;
        _observedViewModel.PropertyChanged += OnViewModelPropertyChanged;

        UpdateResponsiveMode(vm);
        UpdateShellLayout(vm);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(sender is not MainViewModel viewModel) return;

        if(e.PropertyName is nameof(MainViewModel.IsSidebarOpen) or nameof(MainViewModel.IsCompactLayout))
            UpdateShellLayout(viewModel);
    }

    private void OnLayoutStateChanged(object sender, VisualStateChangedEventArgs e)
    {
        _isWideLayout = e.NewState?.Name == "Wide";

        if(DataContext is MainViewModel viewModel)
        {
            UpdateResponsiveMode(viewModel);
            UpdateShellLayout(viewModel);
        }
    }

    private void UpdateResponsiveMode(MainViewModel viewModel)
    {
        double width = ActualWidth;

        if(width <= 0 && XamlRoot is not null) width = XamlRoot.Size.Width;
        if(width <= 0) width = _isWideLayout ? 760 : 759;

        viewModel.ApplyResponsiveLayout(width);
        _isWideLayout = !viewModel.IsCompactLayout;
    }

    private void UpdateShellLayout(MainViewModel viewModel)
    {
        if(_isWideLayout)
        {
            SidebarColumn.Width           = new GridLength(viewModel.IsSidebarOpen ? ExpandedSidebarWidth : CompactSidebarWidth, GridUnitType.Pixel);
            DesktopSidebarHost.Visibility = Visibility.Visible;

            return;
        }

        SidebarColumn.Width           = new GridLength(viewModel.IsSidebarOpen ? ExpandedSidebarWidth : 0, GridUnitType.Pixel);
        DesktopSidebarHost.Visibility = viewModel.IsSidebarOpen ? Visibility.Visible : Visibility.Collapsed;
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
